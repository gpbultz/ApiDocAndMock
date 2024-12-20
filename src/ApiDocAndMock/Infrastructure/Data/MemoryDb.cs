using ApiDocAndMock.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Data
{
    /// <summary>
    /// Simple Dictionary of objects that have been created by api endpoints, used to test workflow of POST, PUT, DELETE and GET endpoints
    /// </summary>
    public class MemoryDb : IMemoryDb
    {
        private readonly Dictionary<Type, List<object>> _store = new();

        public void Add<T>(T item) where T : class
        {
            var type = typeof(T);
            if (!_store.ContainsKey(type))
            {
                _store[type] = new List<object>();
            }
            _store[type].Add(item);
        }

        public IEnumerable<T> GetAll<T>() where T : class
        {
            var type = typeof(T);
            return _store.ContainsKey(type) ? _store[type].Cast<T>() : Enumerable.Empty<T>();
        }

        public T? GetByField<T>(string fieldName, object value) where T : class
        {
            var type = typeof(T);
            if (!_store.ContainsKey(type)) return null;

            var property = type.GetProperty(fieldName);
            if (property == null) throw new ArgumentException($"Field {fieldName} does not exist on type {type.Name}");

            return _store[type].Cast<T>().FirstOrDefault(item => property.GetValue(item)?.Equals(value) == true);
        }

        public bool Update<T>(string fieldName, object value, T updatedItem) where T : class
        {
            var type = typeof(T);
            if (!_store.ContainsKey(type)) return false;

            var property = type.GetProperty(fieldName);
            if (property == null) throw new ArgumentException($"Field {fieldName} does not exist on type {type.Name}");

            var index = _store[type].FindIndex(item => property.GetValue(item)?.Equals(value) == true);
            if (index == -1) return false;

            _store[type][index] = updatedItem;
            return true;
        }

        public bool Delete<T>(string fieldName, object value) where T : class
        {
            var type = typeof(T);
            if (!_store.ContainsKey(type)) return false;

            var property = type.GetProperty(fieldName);
            if (property == null) throw new ArgumentException($"Field {fieldName} does not exist on type {type.Name}");

            var item = _store[type].Cast<T>().FirstOrDefault(i => property.GetValue(i)?.Equals(value) == true);
            if (item == null) return false;

            _store[type].Remove(item);
            return true;
        }
    }
}
