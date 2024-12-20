﻿using ApiDocAndMock.Infrastructure.Configurations;
using Bogus;
using ApiDocAndMock.Application.Interfaces;
using System.Collections;
using System.Reflection;

namespace ApiDocAndMock.Infrastructure.Mocking
{
    public class ApiMockDataFactory : IApiMockDataFactory
    {
        private readonly MockingConfigurations _configurations;

        public ApiMockDataFactory(MockingConfigurations configurations)
        {
            _configurations = configurations;
        }

        /// <summary>
        /// Creates a mocked object. If no mapping is present in the configuration, random values will be applied.
        /// </summary>
        /// <typeparam name="T">Type of object to create.</typeparam>
        /// <param name="nestedCount">Indicates the level of nesting for the object.</param>
        /// <returns>Mocked object.</returns>
        public T CreateMockObject<T>(int nestedCount = 5) where T : class, new()
        {
            var faker = new Faker();
            var instance = Activator.CreateInstance<T>();

            ApplyMockRules(instance, faker, nestedCount);
            return instance;
        }

        /// <summary>
        /// Creates a list of mock objects by repeatedly calling CreateMockObject.
        /// </summary>
        /// <typeparam name="T">Type of object to create.</typeparam>
        /// <param name="count">Number of objects to create.</param>
        /// <param name="nestedCount">Indicates the level of nesting for each object.</param>
        /// <returns>List of mocked objects.</returns>
        public List<T> CreateMockObjects<T>(int count = 1, int nestedCount = 5) where T : class, new()
        {
            var mockObjects = new List<T>();
            for (int i = 0; i < count; i++)
            {
                mockObjects.Add(CreateMockObject<T>(nestedCount));
            }
            return mockObjects;
        }

        private void ApplyMockRules<T>(T instance, Faker faker, int nestedCount) where T : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance), "Instance cannot be null.");
            }

            var type = typeof(T);

            // Retrieve property-specific configurations
            Dictionary<string, Func<Faker, object>> flatConfigs = null;
            Dictionary<string, (Func<Faker, object> Generator, Type NestedType)> nestedConfigs = null;

            if (_configurations.TryGetConfigurationFor<T>(out var propertyConfigs))
            {
                flatConfigs = propertyConfigs.FlatConfigurations;
                nestedConfigs = propertyConfigs.NestedConfigurations;
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanWrite || property.GetIndexParameters().Length > 0)
                {
                    continue; // Skip read-only or indexed properties
                }

                try
                {
                    object value;

                    if (flatConfigs != null && flatConfigs.TryGetValue(property.Name, out var generator))
                    {
                        // Use flat property configuration
                        value = generator(faker);
                    }
                    else if (nestedConfigs != null && nestedConfigs.TryGetValue(property.Name, out var nestedConfig))
                    {
                        // Handle nested lists or objects
                        var (nestedGenerator, nestedType) = nestedConfig;

                        if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType.IsGenericType)
                        {
                            // Nested list
                            value = Enumerable.Range(0, faker.Random.Int(1, 5))
                                              .Select(_ => nestedGenerator(faker))
                                              .ToList();
                        }
                        else
                        {
                            // Nested object
                            value = nestedGenerator(faker);
                        }
                    }
                    else
                    {
                        // Fallback: Dynamically generate data
                        value = GenerateDefaultValueDynamically(property.PropertyType, faker, nestedCount);
                    }

                    property.SetValue(instance, value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting property {property.Name} on type {type.Name}: {ex.Message}");
                }
            }
        }



        private object GenerateNestedList(PropertyInfo property, int count)
        {
            var elementType = property.PropertyType.GetGenericArguments().FirstOrDefault();
            if (elementType == null) return null;

            // Recursive call to CreateMockObjects for nested lists
            var method = typeof(ApiMockDataFactory)
                .GetMethod(nameof(CreateMockObjects), BindingFlags.Public | BindingFlags.Instance)
                ?.MakeGenericMethod(elementType);

            return method?.Invoke(this, new object[] { count, count });
        }

        private object GenerateNestedObject(PropertyInfo property, int nestedCount)
        {
            var nestedType = property.PropertyType;
            if (nestedType == null) return null;

            var method = typeof(ApiMockDataFactory)
                .GetMethod(nameof(CreateMockObject), BindingFlags.Public | BindingFlags.Instance)
                ?.MakeGenericMethod(nestedType);

            return method?.Invoke(this, new object[] { nestedCount });
        }


        private object GenerateDefaultValueDynamically(Type type, Faker faker, int nestedCount = 3)
        {
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
                    array.SetValue(GenerateDefaultValueDynamically(elementType, faker, nestedCount - 1), i);
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
                    list.Add(GenerateDefaultValueDynamically(genericType, faker, nestedCount - 1));
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
                    var key = GenerateDefaultValueDynamically(keyType, faker, nestedCount - 1);
                    var value = GenerateDefaultValueDynamically(valueType, faker, nestedCount - 1);
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

                    var propertyValue = GenerateDefaultValueDynamically(property.PropertyType, faker, nestedCount - 1);
                    property.SetValue(instance, propertyValue);
                }
                return instance;
            }

            // Fallback: return null for unsupported types
            return null;
        }
    }
}
