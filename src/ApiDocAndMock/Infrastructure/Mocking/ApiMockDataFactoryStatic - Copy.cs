using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Application.Models.Responses;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Reflection;

namespace ApiDocAndMock.Infrastructure.Mocking
{
    public  class ApiMockDataFactory : IApiMockDataFactory
    {
        private IServiceProvider _serviceProvider;
        private const int NESTED_COUNT = 20;

        private readonly Dictionary<string, Func<Faker, object>> _defaultFakerRules = new()
        {
            ["Name"] = faker => faker.Name.FullName(),
            ["Email"] = faker => faker.Internet.Email(),
            ["Phone"] = faker => faker.Phone.PhoneNumber(),
            ["Address"] = faker => faker.Address.FullAddress(),
            ["City"] = faker => faker.Address.City(),
            ["Region"] = faker => faker.Address.StateAbbr(),
            ["PostalCode"] = faker => faker.Address.ZipCode(),
            ["Country"] = faker => faker.Address.Country()
        };

        public void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Add a default faker rule to the factory
        /// </summary>
        /// <param name="propertyName">Name of property to apply rule to</param>
        /// <param name="fakerRule">faker type</param>
        public void AddDefaultFakerRule(string propertyName, Func<Faker, object> fakerRule)
        {
            _defaultFakerRules[propertyName] = fakerRule;
        }

        /// <summary>
        /// Creates a mocked object. If no mapping is present in the configuration, random values will be applied.
        /// </summary>
        /// <typeparam name="T">Type of object to create.</typeparam>
        /// <param name="nestedCount">Indicates the level of nesting for the object.</param>
        /// <returns>Mocked object.</returns>
        public T CreateMockObject<T>(int nestedCount = NESTED_COUNT) where T : class, new()
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
        public List<T> CreateMockObjects<T>(int count = 1, int nestedCount = NESTED_COUNT) where T : class, new()
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

            var wrapper = _serviceProvider.GetRequiredService<MockConfigurationsFactoryWrapper>();
            var type = typeof(T);

            // Retrieve the mock rules for the current type
            var mockRules = wrapper?.TryGetConfigurations<T>();

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanWrite || property.GetIndexParameters().Length > 0)
                {
                    continue; // Skip read-only or indexed properties
                }

                try
                {
                    object value;

                    if (mockRules != null && mockRules.TryGetValue(property.Name, out var generator))
                    {
                        // Use the configured generator
                        value = generator(faker);
                    }
                    else if (_defaultFakerRules.TryGetValue(property.Name, out var defaultGenerator))
                    {
                        value = defaultGenerator(faker);
                    }
                    else
                    {
                        // Fallback: Dynamically generate data
                        value = GenerateDefaultValueDynamically(property.Name, property.PropertyType, faker, nestedCount);
                    }

                    property.SetValue(instance, value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting property {property.Name} on type {type.Name}: {ex.Message}");
                }
            }
        }

        private object GenerateDefaultValueDynamically(string name, Type type, Faker faker, int nestedCount = NESTED_COUNT)
        {

            if (_defaultFakerRules.TryGetValue(name, out var fakerRule))
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
                    array.SetValue(GenerateDefaultValueDynamically(name, elementType, faker, nestedCount - 1), i);
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
                    list.Add(GenerateDefaultValueDynamically(name, genericType, faker, nestedCount - 1));
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
                    var key = GenerateDefaultValueDynamically(name, keyType, faker, nestedCount - 1);
                    var value = GenerateDefaultValueDynamically(name, valueType, faker, nestedCount - 1);
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

                    var propertyValue = GenerateDefaultValueDynamically(property.Name, property.PropertyType, faker, nestedCount - 1);
                    property.SetValue(instance, propertyValue);
                }
                return instance;
            }

            // Fallback: return null for unsupported types
            return null;
        }
    }
}

