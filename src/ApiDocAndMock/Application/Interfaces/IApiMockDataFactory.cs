﻿namespace ApiDocAndMock.Application.Interfaces
{
    public interface IApiMockDataFactory
    {
        T CreateMockObject<T>(int nestedCount = 20) where T : class, new();
        List<T> CreateMockObjects<T>(int count = 1, int nestedCount = 20) where T : class, new();
    }
}
