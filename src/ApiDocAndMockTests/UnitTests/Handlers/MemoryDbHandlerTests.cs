using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Data;
using ApiDocAndMock.Infrastructure.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiDocAndMock.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection.Metadata;

namespace ApiDocAndMockTests.UnitTests.Handlers
{
    [TestFixture]
    public class MemoryDbHandlerTests
    {
        private Mock<IMemoryDb> _mockMemoryDb;
        private Mock<IApiMockDataFactory> _mockMockDataFactory;
        private IServiceProvider _serviceProvider;
        private Mock<ILogger<MemoryDb>> _mockLogger;
        private MemoryDbHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockMemoryDb = new Mock<IMemoryDb>();
            _mockMockDataFactory = new Mock<IApiMockDataFactory>();
            _mockLogger = new Mock<ILogger<MemoryDb>>();

            _handler = new MemoryDbHandler(_mockLogger.Object, _mockMemoryDb.Object, _mockMockDataFactory.Object);
        }

        [Test]
        public void CreateMockWithMemoryDb_ShouldReturnLocationPathAndResponse()
        {
            // Arrange
            var request = new CreateItemRequest { Name = "TestItem" };
            var storedItem = new Item();
            var responseItem = new ItemResponse();
            var generatedId = Guid.NewGuid();

            _mockMockDataFactory.Setup(x => x.CreateMockObject<Item>(1)).Returns(storedItem);

            // Act
            var (response, locationPath) = _handler.CreateMockWithMemoryDb<CreateItemRequest, Item, ItemResponse>(
                request,
                storedItem,
                null,
                "Id",
                () => generatedId
            );

            // Assert
            _mockMemoryDb.Verify(db => db.Add(It.Is<Item>(i => i.Name == "TestItem")), Times.Once);
            Assert.That(response.Id, Is.EqualTo(storedItem.Id));
            Assert.That(locationPath, Is.EqualTo($"/items/{generatedId}"));
        }
        [Test]
        public void UpdateMockWithMemoryDb_ShouldUpdateExistingObject()
        {
            // Arrange
            var request = new UpdateItemRequest { Id = Guid.NewGuid(), Name = "UpdatedItem" };
            var existingItem = new Item { Id = request.Id, Name = "OldItem" };
            var updatedItem = new Item { Id = request.Id, Name = "UpdatedItem" };

            _mockMemoryDb
                .Setup(db => db.GetByField<Item>("Id", request.Id))
                .Returns(existingItem);

            // Act
            var (response, outcome) = _handler.UpdateMockWithMemoryDb<UpdateItemRequest, Item, ItemResponse>(
                request, 
                request.Id, 
                existingItem, 
                "Id", 
                "Id", 
                req =>
                {
                    var updated = new Item
                    {
                        Id = req.Id,
                        Name = req.Name
                    };
                    return updated;
                },
                stored => new ItemResponse
                {
                    Id = stored.Id,
                    Name = stored.Name
                }
            );

            // Assert
            _mockMemoryDb.Verify(
                db => db.Update("Id", request.Id, It.Is<Item>(i => i.Name == "UpdatedItem")),
                Times.Once
            );

            Assert.That(response.Name, Is.EqualTo("UpdatedItem"));
            Assert.That(response.Id, Is.EqualTo(request.Id));
            Assert.That(outcome, Is.EqualTo("Return200"));
        }

        [Test]
        public void UpdateMockWithMemoryDb_ShouldCreateMockIfObjectNotFound()
        {
            // Arrange
            var request = new UpdateItemRequest { Id = Guid.NewGuid(), Name = "NewItem" };
            var generatedItem = new Item { Id = request.Id, Name = "GeneratedItem" };

            _mockMockDataFactory
                .Setup(factory => factory.CreateMockObject<Item>(1))
                .Returns(generatedItem);

            // Act
            var (response, outcome) = _handler.UpdateMockWithMemoryDb<UpdateItemRequest, Item, ItemResponse>(
                request,
                request.Id,
                new Item(),  // Simulate no existing object
                "Id",
                "Id",
                null,
                stored => new ItemResponse
                {
                    Id = generatedItem.Id,  // Map Id from stored item to response
                    Name = generatedItem.Name
                }
            );

            // Assert
            Assert.That(response.Name, Is.EqualTo("GeneratedItem"));
            Assert.That(response.Id, Is.EqualTo(request.Id));
            Assert.That(outcome, Is.EqualTo("Return200"));
        }
    }

    internal class UpdateItemRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    internal class ItemResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    internal class Item
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    internal class CreateItemRequest
    {
        public string Name { get; set; }
    }

    
}
