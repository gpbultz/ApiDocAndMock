using ApiDocAndMock.Infrastructure.Mocking;
using Bogus;
using NUnit.Framework;

namespace ApiDocAndMockTests.UnitTests.Factories
{
    public class MockConfigurationsFactoryTests
    {
        private MockConfigurationsFactory _configFactory;
        private Faker _faker;

        [SetUp]
        public void Setup()
        {
            _configFactory = new MockConfigurationsFactory();
            _faker = new Faker();
        }

        [Test]
        public void TryGetConfigurations_ShouldApplyDefaultFakerRules()
        {
            // Act
            var configurations = _configFactory.TryGetConfigurations<TestModel>();

            // Assert
            Assert.That(configurations, Contains.Key("Name"));
            Assert.That(configurations["Name"](_faker), Is.InstanceOf<string>());
        }

        [Test]
        public void RegisterConfiguration_ShouldOverrideDefaultRules()
        {
            // Arrange
            var expectedName = "CustomName";
            _configFactory.RegisterConfiguration<TestModel>(builder =>
                builder.ForProperty("Name", f => expectedName));

            // Act
            var configurations = _configFactory.TryGetConfigurations<TestModel>();

            // Assert
            Assert.That(configurations["Name"](_faker), Is.EqualTo(expectedName));
        }

        [Test]
        public void TryGetConfigurations_ShouldAllowAdditionalFakerRules()
        {
            // Arrange
            _configFactory.AddDefaultFakerRule("CustomProp", f => f.Random.Int(1, 100));

            // Act
            var configurations = _configFactory.TryGetConfigurations<TestModel>();

            // Assert
            Assert.That(configurations, Contains.Key("CustomProp"));
            Assert.That(configurations["CustomProp"](_faker), Is.InstanceOf<int>());
        }

        [Test]
        public void SetDefaultFakerRules_ShouldReplaceExistingRules()
        {
            // Arrange
            _configFactory.AddDefaultFakerRule("CustomProp", f => f.Random.Int(1, 100));

            // Act
            _configFactory.SetDefaultFakerRules(rules =>
            {
                rules["Name"] = faker => "OverriddenName";
                rules["Email"] = faker => "new@example.com";
            });

            var configurations = _configFactory.TryGetConfigurations<TestModel>();

            // Assert
            Assert.That(configurations["Name"](_faker), Is.EqualTo("OverriddenName"));
            Assert.That(configurations["Email"](_faker), Is.EqualTo("new@example.com"));
            Assert.That(configurations.ContainsKey("CustomProp"), Is.False);
        }

        [Test]
        public void SetDefaultFakerRules_ShouldApplyToNewConfigurations()
        {
            // Act
            _configFactory.SetDefaultFakerRules(rules =>
            {
                rules["Name"] = faker => "TestName";
                rules["Age"] = faker => 30;
            });

            var configurations = _configFactory.TryGetConfigurations<TestModel>();

            // Assert
            Assert.That(configurations["Name"](_faker), Is.EqualTo("TestName"));
            Assert.That(configurations["Age"](_faker), Is.EqualTo(30));
        }

        [Test]
        public void SetDefaultFakerRules_ShouldHandleEmptyConfiguration()
        {
            // Act
            _configFactory.SetDefaultFakerRules(rules => { });

            var configurations = _configFactory.TryGetConfigurations<TestModel>();

            // Assert
            Assert.That(configurations.Count, Is.EqualTo(0));
        }

        [Test]
        public void SetDefaultFakerRules_ShouldNotOverrideExplicitConfigurations()
        {
            // Arrange
            _configFactory.RegisterConfiguration<TestModel>(builder =>
                builder.ForProperty("Name", f => "ExplicitName"));

            // Act
            _configFactory.SetDefaultFakerRules(rules =>
            {
                rules["Name"] = faker => "OverriddenName";
            });

            var configurations = _configFactory.TryGetConfigurations<TestModel>();

            // Assert
            Assert.That(configurations["Name"](_faker), Is.EqualTo("ExplicitName")); 
        }



        public class TestModel
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
