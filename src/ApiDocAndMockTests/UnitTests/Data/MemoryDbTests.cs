using ApiDocAndMock.Infrastructure.Data;
using ApiDocAndMockTests.UnitTests.Handlers;
using NUnit.Framework;

namespace ApiDocAndMockTests.UnitTests.Data
{
    [TestFixture]
    public class MemoryDbTests
    {
        private MemoryDb _memoryDb;

        [SetUp]
        public void Setup()
        {
            _memoryDb = new MemoryDb();
        }

        // Test Add and GetAll
        [Test]
        public void Add_ShouldStoreItem()
        {
            var item = new Item { Id = Guid.NewGuid(), Name = "Test Item" };

            _memoryDb.Add(item);

            var allItems = _memoryDb.GetAll<Item>().ToList();

            Assert.That(allItems, Has.Count.EqualTo(1));
            Assert.That(allItems[0].Name, Is.EqualTo("Test Item"));
        }

        // Test GetByField (Existing Item)
        [Test]
        public void GetByField_ShouldReturnItem_WhenMatchingFieldExists()
        {
            var id = Guid.NewGuid();
            var item = new Item { Id = id, Name = "Item1" };
            _memoryDb.Add(item);

            var result = _memoryDb.GetByField<Item>("Id", id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Item1"));
        }

        // Test GetByField (Non-Existent Item)
        [Test]
        public void GetByField_ShouldReturnNull_WhenNoMatchFound()
        {
            var result = _memoryDb.GetByField<Item>("Id", Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        // Test Update (Successful)
        [Test]
        public void Update_ShouldModifyItem_WhenMatchingFieldExists()
        {
            var id = Guid.NewGuid();
            var item = new Item { Id = id, Name = "OldName" };
            _memoryDb.Add(item);

            var updatedItem = new Item { Id = id, Name = "NewName" };

            var updated = _memoryDb.Update("Id", id, updatedItem);

            Assert.That(updated, Is.True);

            var result = _memoryDb.GetByField<Item>("Id", id);
            Assert.That(result.Name, Is.EqualTo("NewName"));
        }

        // Test Update (Item Not Found)
        [Test]
        public void Update_ShouldReturnFalse_WhenNoMatchFound()
        {
            var updatedItem = new Item { Id = Guid.NewGuid(), Name = "DoesNotExist" };

            var updated = _memoryDb.Update("Id", updatedItem.Id, updatedItem);

            Assert.That(updated, Is.False);
        }

        // Test Delete (Successful)
        [Test]
        public void Delete_ShouldRemoveItem_WhenMatchingFieldExists()
        {
            var id = Guid.NewGuid();
            var item = new Item { Id = id, Name = "DeleteMe" };
            _memoryDb.Add(item);

            var deleted = _memoryDb.Delete<Item>("Id", id);

            Assert.That(deleted, Is.True);

            var result = _memoryDb.GetByField<Item>("Id", id);
            Assert.That(result, Is.Null);
        }

        // Test Delete (Item Not Found)
        [Test]
        public void Delete_ShouldReturnFalse_WhenNoMatchFound()
        {
            var deleted = _memoryDb.Delete<Item>("Id", Guid.NewGuid());

            Assert.That(deleted, Is.False);
        }

        // Test Field Does Not Exist
        [Test]
        public void GetByField_ShouldThrowException_WhenFieldDoesNotExist()
        {
            var item = new Item { Id = Guid.NewGuid(), Name = "InvalidFieldTest" };
            _memoryDb.Add(item);

            var ex = Assert.Throws<ArgumentException>(() =>
            {
                _memoryDb.GetByField<Item>("NonExistentField", item.Id);
            });

            Assert.That(ex.Message, Does.Contain("Field NonExistentField does not exist"));
        }

        // Test Add Multiple Items
        [Test]
        public void Add_ShouldSupportMultipleItemsOfSameType()
        {
            var item1 = new Item { Id = Guid.NewGuid(), Name = "Item1" };
            var item2 = new Item { Id = Guid.NewGuid(), Name = "Item2" };

            _memoryDb.Add(item1);
            _memoryDb.Add(item2);

            var allItems = _memoryDb.GetAll<Item>().ToList();

            Assert.That(allItems, Has.Count.EqualTo(2));
        }
    }
}