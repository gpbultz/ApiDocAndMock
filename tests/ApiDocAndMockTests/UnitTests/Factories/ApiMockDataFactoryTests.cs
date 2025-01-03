using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Mocking;
using Bogus;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ApiDocAndMockTests.UnitTests.Factories
{
    public class ApiMockDataFactoryTests
    {
        private ApiMockDataFactory _mockDataFactory;
        private Mock<IServiceProvider> _serviceProviderMock;
        private Mock<IMockConfigurationsFactory> _mockConfigurationsFactory;
        private ILogger<ApiMockDataFactory> _logger;
        [SetUp]
        public void Setup()
        {
            
            _serviceProviderMock = new Mock<IServiceProvider>();
            _mockConfigurationsFactory = new Mock<IMockConfigurationsFactory>();

            _serviceProviderMock.Setup(x => x.GetService(typeof(IMockConfigurationsFactory)))
                                .Returns(_mockConfigurationsFactory.Object);
            var loggerMock = new Mock<ILogger<ApiMockDataFactory>>();
            _logger = loggerMock.Object;

            _mockDataFactory = new ApiMockDataFactory(_serviceProviderMock.Object, _logger);
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
        public void CreateMockObject_ShouldLogPropertyValues()
        {
            // Arrange
            var config = new Dictionary<string, Func<Faker, object>>
            {
                { "Name", faker => "MockedName" }
            };

            _mockConfigurationsFactory.Setup(x => x.TryGetConfigurations<TestModel>())
                                      .Returns(config);

            // Act
            var result = _mockDataFactory.CreateMockObject<TestModel>();

            // Assert
            Assert.That(result.Name, Is.EqualTo("MockedName"));

            Mock.Get(_logger).Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully created mock object of type TestModel")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Mock.Get(_logger).Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Applied configured mock rule for Name")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        public class TestModel
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
