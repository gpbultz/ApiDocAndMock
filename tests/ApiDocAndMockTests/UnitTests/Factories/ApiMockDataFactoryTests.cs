using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Mocking;
using Bogus;
using Moq;
using NUnit.Framework;

namespace ApiDocAndMockTests.UnitTests.Factories
{
    public class ApiMockDataFactoryTests
    {
        private ApiMockDataFactory _mockDataFactory;
        private Mock<IServiceProvider> _serviceProviderMock;
        private Mock<IMockConfigurationsFactory> _mockConfigurationsFactory;

        [SetUp]
        public void Setup()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _mockConfigurationsFactory = new Mock<IMockConfigurationsFactory>();

            _serviceProviderMock.Setup(x => x.GetService(typeof(IMockConfigurationsFactory)))
                                .Returns(_mockConfigurationsFactory.Object);

            _mockDataFactory = new ApiMockDataFactory(_serviceProviderMock.Object);
        }

        [Test]
        public void CreateMockObject_WhenConfigExists_ShouldApplyConfiguredValues()
        {
            // Arrange
            var faker = new Faker();
            var expectedValue = faker.Name.FirstName();
            _mockConfigurationsFactory.Setup(x => x.TryGetConfigurations<TestModel>())
                                      .Returns(new Dictionary<string, Func<Faker, object>>
                                      {
                                          { "Name", f => expectedValue }
                                      });

            // Act
            var result = _mockDataFactory.CreateMockObject<TestModel>();

            // Assert
            Assert.That(result.Name, Is.EqualTo(expectedValue));
        }

        [Test]
        public void CreateMockObject_WhenNoConfig_ShouldApplyDefaultValues()
        {
            // Arrange
            _mockConfigurationsFactory.Setup(x => x.TryGetConfigurations<TestModel>())
                                      .Returns(new Dictionary<string, Func<Faker, object>>());

            // Act
            var result = _mockDataFactory.CreateMockObject<TestModel>();

            // Assert
            Assert.That(result.Name, Is.Not.Null);
            Assert.That(result.Name, Is.InstanceOf<string>());
        }

        [Test]
        public void CreateMockObject_WhenConfigFactoryIsNull_ShouldApplyDefaults()
        {
            // Arrange
            _serviceProviderMock.Setup(x => x.GetService(typeof(IMockConfigurationsFactory)))
                                .Returns(null);

            // Act
            var result = _mockDataFactory.CreateMockObject<TestModel>();

            // Assert
            Assert.That(result.Name, Is.Null);  // Expecting null since no configuration exists
        }


        public class TestModel
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
