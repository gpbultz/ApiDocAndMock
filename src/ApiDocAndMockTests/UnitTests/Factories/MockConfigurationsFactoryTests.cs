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
        public void TryGetConfigurations_WhenNoRulesExist_ShouldApplyOnlyDefaultFakerRules()
        {
            // Act
            var configurations = _configFactory.TryGetConfigurations<TestModel>();

            // Assert
            Assert.That(configurations["Name"](_faker), Is.InstanceOf<string>());
            Assert.That(configurations.ContainsKey("Age"), Is.False);
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

        public class TestModel
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
