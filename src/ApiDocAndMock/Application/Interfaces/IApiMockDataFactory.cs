using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiDocAndMock.Application.Interfaces
{
    public interface IApiMockDataFactory
    {
        void AddDefaultFakerRule(string propertyName, Func<Faker, object> fakerRule);
        T CreateMockObject<T>(int nestedCount = 20) where T : class, new();
        List<T> CreateMockObjects<T>(int count = 1, int nestedCount = 20) where T : class, new();

    }
}
