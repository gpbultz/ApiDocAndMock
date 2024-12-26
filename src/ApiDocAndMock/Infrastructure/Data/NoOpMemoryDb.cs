using ApiDocAndMock.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Infrastructure.Data
{
    public class NoOpMemoryDb : IMemoryDb
    {
        private readonly ILogger<NoOpMemoryDb> _logger;

        public NoOpMemoryDb(ILogger<NoOpMemoryDb> logger)
        {
            _logger = logger;
        }

        public void Add<T>(T item) where T : class
        {
            _logger.LogWarning("Add operation called on NoOpMemoryDb for type {Type}.", typeof(T).Name);
            // No action taken
        }

        public bool Delete<T>(string fieldName, object fieldValue) where T : class
        {
            _logger.LogWarning("Delete operation called on NoOpMemoryDb for type {Type} (Field: {FieldName}, Value: {FieldValue}).",
                                typeof(T).Name, fieldName, fieldValue);
            return false;
        }

        public IEnumerable<T> GetAll<T>() where T : class
        {
            _logger.LogWarning("GetAll operation called on NoOpMemoryDb for type {Type}.", typeof(T).Name);
            return Enumerable.Empty<T>();
        }

        public T? GetByField<T>(string fieldName, object fieldValue) where T : class
        {
            _logger.LogWarning("GetByField operation called on NoOpMemoryDb for type {Type} (Field: {FieldName}, Value: {FieldValue}).",
                                typeof(T).Name, fieldName, fieldValue);
            return null;
        }

        public bool Update<T>(string fieldName, object fieldValue, T updatedItem) where T : class
        {
            _logger.LogWarning("Update operation called on NoOpMemoryDb for type {Type} (Field: {FieldName}, Value: {FieldValue}).",
                                typeof(T).Name, fieldName, fieldValue);
            return false;
        }
    }
}
