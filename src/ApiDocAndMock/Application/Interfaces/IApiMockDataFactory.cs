namespace ApiDocAndMock.Application.Interfaces
{
    public interface IApiMockDataFactory
    {
        T CreateMockObject<T>(int nestedCount = 20) where T : class, new();
        List<T> CreateMockObjects<T>(int count = 1, int nestedCount = 20) where T : class, new();
        object CreateMockByType(Type requestType, int nestedCount = 20);
        Dictionary<string, object> GetPropertyValueDictionary<T>(int nestedCount = 20) where T : class;
    }
}
