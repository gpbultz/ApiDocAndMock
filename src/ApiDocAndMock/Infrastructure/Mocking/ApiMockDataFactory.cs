using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Application.Models.Responses;
using Bogus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Reflection;

namespace ApiDocAndMock.Infrastructure.Mocking
{
    public class ApiMockDataFactory : IApiMockDataFactory
    {
        private const int NESTED_COUNT = 20;
        private readonly IServiceProvider _serviceProvider;
        public ApiMockDataFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a mocked object. If no mapping is present in the configuration, random values will be applied.
        /// </summary>
        /// <typeparam name="T">Type of object to create.</typeparam>
        /// <param name="nestedCount">Indicates the level of nesting for the object.</param>
        /// <returns>Mocked object.</returns>
        public  T CreateMockObject<T>(int nestedCount = NESTED_COUNT) where T : class, new()
        {
            var faker = new Faker();
            var instance = Activator.CreateInstance<T>();
            
            ApplyMockRules(instance, faker, nestedCount);

            if (instance is ApiResponseBase apiResponse)
            {
                // Default to null - will be populated based on request params
                apiResponse.Pagination = null;
                apiResponse.Links = null;
            }

            return instance;
        }

        /// <summary>
        /// Creates a list of mock objects by repeatedly calling CreateMockObject.
        /// </summary>
        /// <typeparam name="T">Type of object to create.</typeparam>
        /// <param name="count">Number of objects to create.</param>
        /// <param name="nestedCount">Indicates the level of nesting for each object.</param>
        /// <returns>List of mocked objects.</returns>
        public  List<T> CreateMockObjects<T>(int count = 1, int nestedCount = NESTED_COUNT) where T : class, new()
        {
            var mockObjects = new List<T>();
            for (int i = 0; i < count; i++)
            {
                mockObjects.Add(CreateMockObject<T>(nestedCount));
            }
            return mockObjects;
        }

        private  void ApplyMockRules<T>(T instance, Faker faker, int nestedCount) where T : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance), "Instance cannot be null.");
            }
            var mockConfigurationsFactory = _serviceProvider.GetService<IMockConfigurationsFactory>();

            var type = typeof(T);

            // Retrieve the mock rules for the current type
            var mockRules = mockConfigurationsFactory?.TryGetConfigurations<T>();

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanWrite || property.GetIndexParameters().Length > 0)
                {
                    continue; // Skip read-only or indexed properties
                }

                try
                {
                    object value;

                    if (mockRules.TryGetValue(property.Name, out var generator))
                    {
                        value = generator(faker);
                    }
                    else
                    {
                        value = GenerateDefaultValueDynamically(property.Name, property.PropertyType, faker, mockRules, nestedCount);
                    }

                    property.SetValue(instance, value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting property {property.Name} on type {type.Name}: {ex.Message}");
                }
            }
        }

        private  object GenerateDefaultValueDynamically(string name, Type type, Faker faker, Dictionary<string, Func<Faker, object>> configurations, int nestedCount = NESTED_COUNT)
        {

            if (configurations.TryGetValue(name, out var fakerRule))
            {
                return fakerRule(faker);
            }

            // Prevent deep recursion for nested objects
            if (nestedCount <= 0)
            {
                return null;
            }

            // Handle nullable types
            if (Nullable.GetUnderlyingType(type) != null)
            {
                type = Nullable.GetUnderlyingType(type);
            }

            // Handle common primitives and built-in types
            if (type == typeof(string)) return faker.Lorem.Word();
            if (type == typeof(Guid)) return Guid.NewGuid();
            if (type == typeof(int)) return faker.Random.Int(0, 100);
            if (type == typeof(long)) return faker.Random.Long(0, 1000);
            if (type == typeof(short)) return faker.Random.Short(0, 100);
            if (type == typeof(uint)) return faker.Random.UInt();
            if (type == typeof(ulong)) return faker.Random.ULong();
            if (type == typeof(ushort)) return faker.Random.UShort();
            if (type == typeof(float)) return faker.Random.Float(0.0f, 100.0f);
            if (type == typeof(double)) return faker.Random.Double(0.0, 100.0);
            if (type == typeof(decimal)) return faker.Random.Decimal(0.0m, 100.0m);
            if (type == typeof(bool)) return faker.Random.Bool();
            if (type == typeof(char)) return faker.Random.Char('a', 'z');
            if (type == typeof(byte)) return faker.Random.Byte();
            if (type == typeof(sbyte)) return faker.Random.SByte();
            if (type == typeof(DateTime)) return faker.Date.Between(DateTime.Now.AddYears(-1), DateTime.Now);
            if (type == typeof(TimeSpan)) return faker.Date.Timespan();

            // Handle enums
            if (type.IsEnum)
            {
                var values = Enum.GetValues(type);
                return values.GetValue(faker.Random.Int(0, values.Length - 1));
            }

            // Handle arrays
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var array = Array.CreateInstance(elementType, faker.Random.Int(1, 5));
                for (var i = 0; i < array.Length; i++)
                {
                    array.SetValue(GenerateDefaultValueDynamically(name, elementType, faker, configurations, nestedCount - 1), i);
                }
                return array;
            }

            // Handle collections (e.g., List<T>, IEnumerable<T>)
            if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType)
            {
                var genericType = type.GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(genericType);
                var list = (IList)Activator.CreateInstance(listType);
                for (var i = 0; i < faker.Random.Int(1, 5); i++)
                {
                    list.Add(GenerateDefaultValueDynamically(name, genericType, faker, configurations, nestedCount - 1));
                }
                return list;
            }

            // Handle dictionaries
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];
                var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                var dictionary = (IDictionary)Activator.CreateInstance(dictionaryType);

                for (var i = 0; i < faker.Random.Int(1, 5); i++)
                {
                    var key = GenerateDefaultValueDynamically(name, keyType, faker, configurations, nestedCount - 1);
                    var value = GenerateDefaultValueDynamically(name, valueType, faker, configurations, nestedCount - 1);
                    dictionary.Add(key, value);
                }

                return dictionary;
            }

            // Handle complex objects
            if (type.IsClass && type != typeof(string))
            {
                var instance = Activator.CreateInstance(type);
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!property.CanWrite || property.GetIndexParameters().Length > 0)
                    {
                        continue; // Skip read-only or indexed properties
                    }

                    var propertyValue = GenerateDefaultValueDynamically(property.Name, property.PropertyType, faker, configurations, nestedCount - 1);
                    property.SetValue(instance, propertyValue);
                }
                return instance;
            }

            // Fallback: return null for unsupported types
            return null;
        }
    }
}

