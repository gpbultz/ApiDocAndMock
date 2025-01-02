using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Configurations;
using ApiDocAndMock.Infrastructure.Utilities;
using Bogus;
using Moq;
using NUnit.Framework;

namespace ApiDocAndMockTests.UnitTests.Infrastructure
{
    public class MockConfigurationBuilderTests
    {
        private MockConfigurationBuilder<TestModel> _builder;
        private Faker _faker;

        [SetUp]
        public void Setup()
        {
            _builder = new MockConfigurationBuilder<TestModel>();
            _faker = new Faker();
        }

        [Test]
        public void ForProperty_ShouldConfigurePropertyWithGenerator()
        {
            // Arrange
            var expectedName = "GeneratedName";
            _builder.ForProperty("Name", f => expectedName);

            // Act
            var configurations = _builder.Build();

            // Assert
            Assert.That(configurations, Contains.Key("Name"));
            Assert.That(configurations["Name"](_faker), Is.EqualTo(expectedName));
        }

        [Test]
        public void ForPropertyObject_ShouldCreateNestedObject()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDataFactory = new Mock<IApiMockDataFactory>();

            // Simulate the nested object creation using the mock factory
            mockDataFactory.Setup(x => x.CreateMockObject<NestedModel>(It.IsAny<int>()))
                           .Returns(new NestedModel { NestedName = "NestedGeneratedName" });

            // Set the service provider to return the mock factory
            mockServiceProvider.Setup(x => x.GetService(typeof(IApiMockDataFactory)))
                               .Returns(mockDataFactory.Object);

            ServiceProviderHelper.SetServiceProvider(mockServiceProvider.Object);  // Fixed error

            var nestedBuilder = new MockConfigurationBuilder<NestedModel>()
                .ForProperty("NestedName", f => "NestedGeneratedName");

            _builder.ForPropertyObject<NestedModel>("NestedModel");

            // Act
            var configurations = _builder.Build();
            var nestedObject = configurations["NestedModel"](_faker) as NestedModel;

            // Assert
            Assert.That(nestedObject, Is.Not.Null);
            Assert.That(nestedObject.NestedName, Is.EqualTo("NestedGeneratedName"));
        }



        [Test]
        public void ForPropertyObjectList_ShouldCreateListOfNestedObjects()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDataFactory = new Mock<IApiMockDataFactory>();

            // Simulate list creation
            var nestedModels = new List<NestedModel>
            {
                new NestedModel { NestedName = "NestedGeneratedName1" },
                new NestedModel { NestedName = "NestedGeneratedName2" },
                new NestedModel { NestedName = "NestedGeneratedName3" }
            };

            // Mock the list generation to return the nestedModels list
            mockDataFactory.Setup(x => x.CreateMockObjects<NestedModel>(It.IsAny<int>(), It.IsAny<int>()))
                           .Returns(nestedModels);

            // Set up the service provider to return the mock factory
            mockServiceProvider.Setup(x => x.GetService(typeof(IApiMockDataFactory)))
                               .Returns(mockDataFactory.Object);

            ServiceProviderHelper.SetServiceProvider(mockServiceProvider.Object);  // Ensure the provider is set

            _builder.ForPropertyObjectList<NestedModel>("NestedList", 3);

            // Act
            var configurations = _builder.Build();
            var nestedList = configurations["NestedList"](_faker) as List<NestedModel>;

            // Assert
            Assert.That(nestedList, Is.Not.Null);
            Assert.That(nestedList.Count, Is.EqualTo(3));
            Assert.That(nestedList[0].NestedName, Is.EqualTo("NestedGeneratedName1"));
        }



        [Test]
        public void ForPropertyTuple_ShouldGenerateTupleWithCorrectValues()
        {
            // Arrange
            _builder.ForPropertyTuple(
                "Coordinates",
                f => f.Random.Double(0, 100),
                f => f.Random.Double(0, 100));

            // Act
            var configurations = _builder.Build();
            var tuple = configurations["Coordinates"](_faker) as Tuple<double, double>;

            // Assert
            Assert.That(tuple, Is.Not.Null);
            Assert.That(tuple.Item1, Is.InstanceOf<double>());
            Assert.That(tuple.Item2, Is.InstanceOf<double>());
        }

        [Test]
        public void ForPropertyDictionary_ShouldGenerateDictionaryWithSpecifiedRules()
        {
            // Arrange
            _builder.ForPropertyDictionary(
                "Mappings",
                3,
                f => f.Random.Word(),
                f => f.Random.Int(1, 100));

            // Act
            var configurations = _builder.Build();
            var dictionary = configurations["Mappings"](_faker) as Dictionary<string, int>;

            // Assert
            Assert.That(dictionary, Is.Not.Null);
            Assert.That(dictionary.Count, Is.EqualTo(3));
        }

        // Models for testing
        public class TestModel
        {
            public string Name { get; set; }
            public NestedModel NestedModel { get; set; }
            public List<NestedModel> NestedList { get; set; }
            public Tuple<double, double> Coordinates { get; set; }
            public Dictionary<string, int> Mappings { get; set; }
        }

        public class NestedModel
        {
            public string NestedName { get; set; }
        }
    }
}
