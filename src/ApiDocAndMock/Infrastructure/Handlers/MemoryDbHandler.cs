using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Data;
using ApiDocAndMock.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace ApiDocAndMock.Infrastructure.Handlers
{
    public class MemoryDbHandler : IMemoryDbHandler
    {
        private readonly ILogger<MemoryDb> _logger;
        private readonly IMemoryDb _memoryDb;
        private readonly IApiMockDataFactory _mockDataFactory;

        public MemoryDbHandler(ILogger<MemoryDb> logger, IMemoryDb memoryDb, IApiMockDataFactory apiMockDataFactory)
        {
            _logger = logger;
            _memoryDb = memoryDb;
            _mockDataFactory = apiMockDataFactory;
        }

        public (TResponse response, string locationPath) CreateMockWithMemoryDb<TRequest, TStored, TResponse>(TRequest request, TStored stored, Func<TRequest, TStored>? customMapper = null, string idFieldName = "Id", Func<object>? generateId = null, Func<TStored, string>? locationPathBuilder = null)
            where TRequest : class
            where TStored : class, new()
            where TResponse : class, new()
        {
            var newId = generateId?.Invoke() ?? Guid.NewGuid();

            TStored storedObject = customMapper != null ? customMapper(request) : MapRequestToStored(request, stored);

            typeof(TStored).GetProperty(idFieldName)?.SetValue(storedObject, newId);

            _memoryDb.Add(storedObject);

            var response = new TResponse();
            typeof(TResponse).GetProperty(idFieldName)?.SetValue(response, newId);
            var locationPath = locationPathBuilder != null ? locationPathBuilder(storedObject) : $"/{typeof(TStored).Name.ToLower()}s/{newId}";

            return (response, locationPath);
        }

        public (TResponse response, string outcome) UpdateMockWithMemoryDb<TRequest, TStored, TResponse>(TRequest request, object id, TStored updatedObject, string idFieldName = "Id", string queryIdFieldName = "Id", Func<TRequest, TStored>? customMapper = null,
                                                                                                            Func<TStored, TResponse>? responseMapper = null, string defaultMethodOutcome = "Return200", string? locationPath = null)
            where TRequest : class
            where TStored : class, new()
            where TResponse : class, new()
        {
            if (customMapper != null)
            {
                updatedObject = customMapper(request);
            }
            else
            {
                updatedObject = MapRequestToStored(request, updatedObject);
            }

            _memoryDb.Update(queryIdFieldName, id, updatedObject);

            var response = responseMapper != null
                ? responseMapper(updatedObject)
                : _mockDataFactory.CreateMockObject<TResponse>() ?? new TResponse();

            return (response, defaultMethodOutcome);
        }

        public (TResponse response, DefaultMethodBehaviour behaviour) DeleteMockWithMemoryDb<TStored, TResponse>(object id, string idFieldName = "Id", Func<TStored, TResponse>? responseMapper = null, DefaultMethodBehaviour defaultMethodBehaviour = DefaultMethodBehaviour.Return204)
            where TStored : class, new()
            where TResponse : class, new()
        {
            var existingObject = _memoryDb.GetByField<TStored>(idFieldName, id);

            // Handle 'not found' scenarios
            if (existingObject == null)
            {
                if (defaultMethodBehaviour == DefaultMethodBehaviour.Return204)
                {
                    return (null, DefaultMethodBehaviour.Return204);  // NoContent
                }

                var mockResponse = _mockDataFactory.CreateMockObject<TResponse>();
                return (mockResponse, DefaultMethodBehaviour.Return200);
            }

            // Delete the object
            _memoryDb.Delete<TStored>(idFieldName, id);

            // Generate response if needed
            var response = responseMapper != null
                ? responseMapper(existingObject)
                : _mockDataFactory.CreateMockObject<TResponse>();

            return (response, defaultMethodBehaviour);
        }

        public (T? item, NotFoundBehaviour behaviour) GetMockFromMemoryDb<T>(object id, string idFieldName = "Id", NotFoundBehaviour defaultBehaviour = NotFoundBehaviour.Return404, T mockedItem = null)
            where T : class, new()
        {
            var item = _memoryDb.GetByField<T>(idFieldName, id);

            if (item == null)
            {
                if (defaultBehaviour == NotFoundBehaviour.ReturnMockIfNotFound)
                {

                    typeof(T).GetProperty(idFieldName)?.SetValue(mockedItem, id);

                    return (mockedItem, NotFoundBehaviour.ReturnMockIfNotFound);
                }

                return (null, NotFoundBehaviour.Return404);
            }

            return (item, default);
        }


        private static TStored MapRequestToStored<TRequest, TStored>(TRequest request, TStored storedObject)
            where TRequest : class
            where TStored : class
        {
            foreach (var property in typeof(TRequest).GetProperties())
            {
                var targetProperty = typeof(TStored).GetProperty(property.Name);
                targetProperty?.SetValue(storedObject, property.GetValue(request));
            }
            return storedObject;
        }
    }


}
