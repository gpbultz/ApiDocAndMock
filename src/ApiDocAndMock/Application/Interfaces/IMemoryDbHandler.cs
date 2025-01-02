using ApiDocAndMock.Shared.Enums;

namespace ApiDocAndMock.Application.Interfaces
{
    public interface IMemoryDbHandler
    {
        (TResponse response, string locationPath) CreateMockWithMemoryDb<TRequest, TStored, TResponse>(TRequest request, TStored stored, Func<TRequest, TStored>? customMapper = null, string idFieldName = "Id", Func<object>? generateId = null, Func<TStored, string>? locationPathBuilder = null)
            where TRequest : class
            where TStored : class, new()
            where TResponse : class, new();

        (TResponse response, string outcome) UpdateMockWithMemoryDb<TRequest, TStored, TResponse>(TRequest request, object id, TStored existingObject, string idFieldName = "Id", string queryIdFieldName = "Id", Func<TRequest, TStored>? customMapper = null,
                                                                                                            Func<TStored, TResponse>? responseMapper = null, string defaultMethodOutcome = "Return200", string? locationPath = null)
            where TRequest : class
            where TStored : class, new()
            where TResponse : class, new();

        (TResponse response, DefaultMethodBehaviour behaviour) DeleteMockWithMemoryDb<TStored, TResponse>(object id, string idFieldName = "Id", Func<TStored, TResponse>? responseMapper = null, DefaultMethodBehaviour defaultMethodBehaviour = DefaultMethodBehaviour.Return204)
            where TStored : class, new()
            where TResponse : class, new();

        (T? item, NotFoundBehaviour behaviour) GetMockFromMemoryDb<T>(object id, string idFieldName = "Id", NotFoundBehaviour defaultBehaviour = NotFoundBehaviour.Return404, T mockedItem = null)
            where T : class, new();



    }


}
