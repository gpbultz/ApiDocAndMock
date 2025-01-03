using ApiDocAndMock.Application.Interfaces;
using ApiDocAndMock.Infrastructure.Configurations;
using Bogus;
using Microsoft.Extensions.DependencyInjection;

namespace ApiDocAndMock.Infrastructure.Mocking
{
    public class MockConfigurationsFactory : IMockConfigurationsFactory
    {
        private readonly Dictionary<Type, Dictionary<string, Func<Faker, object>>> _configurations = new();
        private Dictionary<string, Func<Faker, object>> _defaultFakerRules = new();

        public MockConfigurationsFactory()
        {
            _defaultFakerRules = new()
            {
                ["Name"] = faker => faker.Name.FullName(),
                ["FirstName"] = faker => faker.Name.FirstName(),
                ["LastName"] = faker => faker.Name.LastName(),
                ["FullName"] = faker => faker.Name.FullName(),
                ["Email"] = faker => faker.Internet.Email(),
                ["Phone"] = faker => faker.Phone.PhoneNumber(),
                ["Address"] = faker => faker.Address.StreetAddress(),
                ["City"] = faker => faker.Address.City(),
                ["Region"] = faker => faker.Address.StateAbbr(),
                ["PostalCode"] = faker => faker.Address.ZipCode(),
                ["Country"] = faker => faker.Address.Country(),
                ["CompanyName"] = faker => faker.Company.CompanyName(),
                ["Street"] = faker => faker.Address.StreetName(),
                ["Website"] = faker => faker.Internet.Url(),
                ["Username"] = faker => faker.Internet.UserName(),
                ["JobTitle"] = faker => faker.Name.JobTitle(),
                ["Position"] = faker => faker.Name.JobTitle(),
                ["Department"] = faker => faker.Commerce.Department(),
                ["Description"] = faker => faker.Lorem.Sentence(),
                ["Notes"] = faker => faker.Lorem.Paragraph(),
                ["CreatedAt"] = faker => faker.Date.Past(),
                ["UpdatedAt"] = faker => faker.Date.Recent(),
                ["Price"] = faker => faker.Commerce.Price(),
                ["Quantity"] = faker => faker.Random.Int(1, 100),
                ["OrderNumber"] = faker => faker.Random.Int(1000, 9999),
                ["Status"] = faker => faker.PickRandom(new[] { "Active", "Pending", "Cancelled" }),
                ["Username"] = faker => faker.Internet.UserName(),
                ["Password"] = faker => faker.Internet.Password(12), 
                ["ProfilePicture"] = faker => faker.Internet.Avatar(),
                ["Biography"] = faker => faker.Lorem.Paragraph(),
                ["DateOfBirth"] = faker => faker.Date.Past(30, DateTime.Now.AddYears(-18)),
                ["Age"] = faker => faker.Random.Int(18, 80),
                ["Nationality"] = faker => faker.Address.Country(),
                ["Language"] = faker => faker.PickRandom(new[] { "English", "Spanish", "French", "German" }),
                ["Role"] = faker => faker.PickRandom(new[] { "Admin", "User", "Manager", "Guest" }),
                ["ProductName"] = faker => faker.Commerce.ProductName(),
                ["Category"] = faker => faker.Commerce.Categories(1)[0],
                ["Brand"] = faker => faker.Company.CompanyName(),
                ["SKU"] = faker => faker.Random.AlphaNumeric(10).ToUpper(),
                ["Barcode"] = faker => faker.Random.ReplaceNumbers("###########"),
                ["Price"] = faker => faker.Commerce.Price(),
                ["Discount"] = faker => faker.Random.Double(0, 50),
                ["Stock"] = faker => faker.Random.Int(0, 1000),
                ["ProductDescription"] = faker => faker.Commerce.ProductDescription(),
                ["ReleaseDate"] = faker => faker.Date.Past(3),
                ["WarrantyPeriod"] = faker => $"{faker.Random.Int(1, 5)} years",
                ["AccountNumber"] = faker => faker.Finance.Account(),
                ["IBAN"] = faker => faker.Finance.Iban(),
                ["SWIFT"] = faker => faker.Finance.Bic(),
                ["TransactionId"] = faker => faker.Random.Guid(),
                ["Amount"] = faker => faker.Finance.Amount(),
                ["Currency"] = faker => faker.Finance.Currency().Code,
                ["PaymentMethod"] = faker => faker.PickRandom(new[] { "Credit Card", "PayPal", "Bank Transfer" }),
                ["CreditCardNumber"] = faker => faker.Finance.CreditCardNumber(),
                ["ExpirationDate"] = faker => faker.Date.Future(3),
                ["SecurityCode"] = faker => faker.Random.Int(100, 999),
                ["ProjectName"] = faker => faker.Company.CatchPhrase(),
                ["TaskTitle"] = faker => faker.Lorem.Sentence(),
                ["TaskStatus"] = faker => faker.PickRandom(new[] { "Pending", "In Progress", "Completed" }),
                ["Deadline"] = faker => faker.Date.Future(1),
                ["Priority"] = faker => faker.PickRandom(new[] { "High", "Medium", "Low" }),
                ["AssignedTo"] = faker => faker.Name.FullName(),
                ["Team"] = faker => faker.Commerce.Department(),
                ["Milestone"] = faker => faker.Company.Bs(),
                ["Latitude"] = faker => faker.Address.Latitude().ToString(),
                ["Longitude"] = faker => faker.Address.Longitude().ToString(),
                ["Coordinates"] = faker => $"{faker.Address.Latitude()}, {faker.Address.Longitude()}",
                ["WarehouseLocation"] = faker => faker.Address.City(),
                ["ShipmentStatus"] = faker => faker.PickRandom(new[] { "Pending", "Shipped", "Delivered", "Returned" }),
                ["TrackingNumber"] = faker => faker.Random.Replace("1Z##########"),
                ["DeliveryDate"] = faker => faker.Date.Future(1),
                ["Carrier"] = faker => faker.Company.CompanyName()
            };
        }

        public void RegisterConfiguration<T>(Action<MockConfigurationBuilder<T>> configure) where T : class
        {
            var builder = new MockConfigurationBuilder<T>();
            configure(builder);
            _configurations[typeof(T)] = builder.Build();
        }

        public void AddDefaultFakerRule(string property, Func<Faker, object> generator)
        {
            _defaultFakerRules[property] = generator;
        }

        public void SetDefaultFakerRules(Action<Dictionary<string, Func<Faker, object>>> configure)
        {
            _defaultFakerRules.Clear();
            configure(_defaultFakerRules);
        }

        public Dictionary<string, Func<Faker, object>> TryGetConfigurations<T>() where T : class
        {
            var mergedRules = new Dictionary<string, Func<Faker, object>>(_defaultFakerRules);

            if (_configurations.TryGetValue(typeof(T), out var rules))
            {
                foreach (var rule in rules)
                {
                    mergedRules[rule.Key] = rule.Value;
                }
            }

            return mergedRules;
        }
    }

}
