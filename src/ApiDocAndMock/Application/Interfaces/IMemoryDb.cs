namespace ApiDocAndMock.Application.Interfaces
{
    public interface IMemoryDb
    {
        void Add<T>(T item) where T : class;
        T? GetByField<T>(string fieldName, object fieldValue) where T : class;
        bool Update<T>(string fieldName, object fieldValue, T updatedItem) where T : class;
        bool Delete<T>(string fieldName, object fieldValue) where T : class;
        IEnumerable<T> GetAll<T>() where T : class;
    }
}
