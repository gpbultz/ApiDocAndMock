using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Application.Models.Responses;
using Bogus;
using Bogus.Bson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace ApiDocAndMock.Infrastructure.Mocking
{
    public class ApiMockDataFactory : IApiMockDataFactory
    {
        private const int NESTED_COUNT = 20;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ApiMockDataFactory> _logger;

        public ApiMockDataFactory(IServiceProvider serviceProvider, ILogger<ApiMockDataFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
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

            var hashCode = instance.GetHashCode();
            var propertyValues = GetPropertyValues(instance);

            _logger.LogInformation($"Successfully created mock object of type {typeof(T).Name} with hashcode {hashCode}. Properties: {propertyValues}");


            return instance;
        }

        /// <summary>
        /// Creates a mocked mediatr IRequest.
        /// </summary>
        /// <param name="requestType"></param>
        /// <param name="nestedCount"></param>
        /// <returns></returns>
        public object CreateMockByType(Type requestType, int nestedCount = NESTED_COUNT)
        {
            var method = typeof(ApiMockDataFactory)
                .GetMethod(nameof(CreateMockObject), BindingFlags.Public | BindingFlags.Instance)
                .MakeGenericMethod(requestType);

            // Invoke the method dynamically
            var instance = method.Invoke(this, new object[] { nestedCount });

            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to create mock for {requestType.Name}.");
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

        public Dictionary<string, object> GetPropertyValueDictionary<T>(int nestedCount = NESTED_COUNT) where T : class
        {
            var result = new Dictionary<string, object>();

            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead);

            foreach (var property in properties)
            {
                try
                {
                    var value = ApplyMockRule(property, nestedCount);
                    result[property.Name] = value;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to generate value for property '{property.Name}' of type '{typeof(T).Name}'.");
                    result[property.Name] = null;
                }
            }

            return result;
        }

        private void ApplyMockRules<T>(T instance, Faker faker, int nestedCount) where T : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance), "Instance cannot be null.");
            }
            var mockConfigurationsFactory = _serviceProvider.GetService<IMockConfigurationsFactory>();

            var type = typeof(T);
            _logger.LogDebug($"Applying mock rules for {type.Name} (HashCode: {instance.GetHashCode()})");

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

                    if (mockRules != null && mockRules.TryGetValue(property.Name, out var generator))
                    {
                        var generatedValue = generator(faker);

                        // Check if the generated value type matches the property type
                        if (generatedValue != null && property.PropertyType.IsAssignableFrom(generatedValue.GetType()))
                        {
                            value = generatedValue;
                            _logger.LogDebug($"Applied configured mock rule for {property.Name} on {type.Name}. Value: {value}");
                        }
                        else
                        {
                            value = GenerateDefaultValueDynamically(property.Name, property.PropertyType, faker, mockRules, nestedCount);
                            _logger.LogDebug($"Generated default value for {property.Name} on {type.Name}. Value: {value}");
                        }
                    }
                    else
                    {
                        value = GenerateDefaultValueDynamically(property.Name, property.PropertyType, faker, mockRules, nestedCount);
                        _logger.LogDebug($"Generated default value for {property.Name} on {type.Name}. Value: {value}");
                    }

                    property.SetValue(instance, value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to set property {property.Name} on {type.Name}");
                }
            }
        }

        private object ApplyMockRule(PropertyInfo property, int nestedCount = NESTED_COUNT)
        {
            var faker = new Faker();
            var propertyName = property.Name;
            var propertyType = property.PropertyType;

            var mockConfigurationsFactory = _serviceProvider.GetService<IMockConfigurationsFactory>();
            var mockRules = mockConfigurationsFactory?.TryGetConfigurations<object>();

            if (mockRules != null && mockRules.TryGetValue(propertyName, out var generator))
            {
                _logger.LogDebug($"Applying configured mock rule for '{propertyName}' of type '{propertyType.Name}'.");
                return generator(faker);
            }

            _logger.LogDebug($"No specific rule for '{propertyName}'. Generating default value.");
            return GenerateDefaultValueDynamically(
                propertyName,
                propertyType,
                faker,
                mockRules,
                nestedCount
            );
        }

        private object GenerateDefaultValueDynamically(string name, Type type, Faker faker, Dictionary<string, Func<Faker, object>> configurations, int nestedCount = NESTED_COUNT)
        {
            if (configurations != null && configurations.TryGetValue(name, out var fakerRule))
            {
                var ruleResult = fakerRule(faker);
                if (type.IsAssignableFrom(ruleResult.GetType()))
                {
                    return ruleResult;
                }
            }

            if (nestedCount <= 0)
            {
                return null;
            }

            if (Nullable.GetUnderlyingType(type) != null)
            {
                type = Nullable.GetUnderlyingType(type);
            }

            if (type == typeof(string)) return faker.Lorem.Word();
            if (type == typeof(Guid)) return Guid.NewGuid();
            if (type == typeof(int)) return faker.Random.Int(0, 100);
            if (type == typeof(long)) return faker.Random.Long(0, 1000);
            if (type == typeof(bool)) return faker.Random.Bool();
            if (type == typeof(DateTime)) return faker.Date.Between(DateTime.Now.AddYears(-1), DateTime.Now);

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

            // Handle dictionaries with complex types
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

                    if (key != null && value != null)
                    {
                        dictionary.Add(key, value);
                    }
                }
                return dictionary;
            }

            // Handle tuples
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<,>))
            {
                var genericArguments = type.GetGenericArguments();
                var item1 = GenerateDefaultValueDynamically(name, genericArguments[0], faker, configurations, nestedCount - 1);
                var item2 = GenerateDefaultValueDynamically(name, genericArguments[1], faker, configurations, nestedCount - 1);

                return Activator.CreateInstance(type, item1, item2);
            }

            

            // Handle concurrent dictionaries
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>))
            {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];
                var concurrentDictType = typeof(ConcurrentDictionary<,>).MakeGenericType(keyType, valueType);
                var concurrentDict = (IDictionary)Activator.CreateInstance(concurrentDictType);

                for (var i = 0; i < faker.Random.Int(1, 5); i++)
                {
                    var key = GenerateDefaultValueDynamically(name, keyType, faker, configurations, nestedCount - 1);
                    var value = GenerateDefaultValueDynamically(name, valueType, faker, configurations, nestedCount - 1);
                    if (key != null && value != null)
                    {
                        concurrentDict.Add(key, value);
                    }
                }
                return concurrentDict;
            }

            // Handle observable collections and collections
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(ObservableCollection<>) || type.GetGenericTypeDefinition() == typeof(Collection<>)))
            {
                var genericType = type.GetGenericArguments()[0];
                var collectionType = type.GetGenericTypeDefinition().MakeGenericType(genericType);

                var collection = (IList)Activator.CreateInstance(collectionType);

                for (var i = 0; i < faker.Random.Int(1, 5); i++)
                {
                    var item = Activator.CreateInstance(genericType);

                    if (item != null)
                    {
                        collection.Add(GenerateDefaultValueDynamically(name, genericType, faker, configurations, nestedCount - 1));
                    }
                }

                return collection;
            }

            // Handle queues
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Queue<>))
            {
                var genericType = type.GetGenericArguments()[0];
                var queueType = typeof(Queue<>).MakeGenericType(genericType);
                var queue = Activator.CreateInstance(queueType);

                for (var i = 0; i < faker.Random.Int(1, 5); i++)
                {
                    var item = GenerateDefaultValueDynamically(name, genericType, faker, configurations, nestedCount - 1);
                    queueType.GetMethod("Enqueue")?.Invoke(queue, new[] { item });
                }
                return queue;
            }

            // Handle stacks
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Stack<>))
            {
                var genericType = type.GetGenericArguments()[0];
                var stackType = typeof(Stack<>).MakeGenericType(genericType);
                var stack = Activator.CreateInstance(stackType);

                for (var i = 0; i < faker.Random.Int(1, 5); i++)
                {
                    var item = GenerateDefaultValueDynamically(name, genericType, faker, configurations, nestedCount - 1);
                    stackType.GetMethod("Push")?.Invoke(stack, new[] { item });
                }
                return stack;
            }

            // Handle lists/collections
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

            // Handle complex objects (Ensure they are not null)
            if (type.IsClass && type != typeof(string))
            {
                var instance = Activator.CreateInstance(type);
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!property.CanWrite || property.GetIndexParameters().Length > 0)
                    {
                        continue;
                    }
                    var propertyValue = GenerateDefaultValueDynamically(property.Name, property.PropertyType, faker, configurations, nestedCount - 1);
                    property.SetValue(instance, propertyValue);
                }
                return instance;
            }

            // Fallback to empty object instead of null
            return Activator.CreateInstance(type) ?? null;
        }

        private string GetPropertyValues<T>(T instance)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var values = properties
                .Select(p => $"{p.Name}: {p.GetValue(instance)?.ToString() ?? "null"}")
                .ToList();

            return string.Join(", ", values);
        }
    }
}

