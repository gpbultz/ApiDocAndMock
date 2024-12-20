namespace ApiDocAndMock.Application.Interfaces
{
    public interface IApiMockDataFactory
    {
        T CreateMockObject<T>(int nestedCount = 5) where T : class, new();
        List<T> CreateMockObjects<T>(int count = 1, int nestedCount = 5) where T : class, new();
    }
}
