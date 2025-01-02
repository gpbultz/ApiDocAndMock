# Release 1.2.0 – Complex Type Support for Dictionary and Tuple

---

## What's New
### 🚀 Complex Type Support for Dictionary and Tuple
- **Dictionary<TKey, TValue> Support**:  
  - Added support for mock generation of dictionaries with either primitive or complex object values.  
  - If the value type (`TValue`) is a complex object, its faker rules are applied automatically if a configuration exists.  
  - This eliminates the need to redefine faker rules for each dictionary—simply register the object configuration once.  

  **Example:**  
  ```csharp
  config.RegisterConfiguration<Appointment>(cfg =>
  {
      cfg
          .ForProperty("DateOfAppointment", faker => faker.Date.Future())
          .ForProperty("Description", faker => "Meeting about " + faker.Company.CatchPhrase());
  });

  config.RegisterConfiguration<GetContactsResponse>(cfg =>
  {
      cfg
          .ForPropertyObjectList<GetContactByIdResponse>("Contacts", 5)
          .ForPropertyDictionary<Guid, Appointment>("Appointments", 3,
              faker => Guid.NewGuid(),
              faker => ServiceProviderHelper.GetService<IApiMockDataFactory>().CreateMockObject<Appointment>());
  });
  ```

- **Tuple<T1, T2> Support**:  
  - Introduced support for `Tuple<T1, T2>` properties within mock configurations.  
  - The configuration can apply faker rules to both tuple values individually.  

  **Example:**  
  ```csharp
  config.RegisterConfiguration<Location>(cfg =>
  {
      cfg
          .ForPropertyTuple("Coordinates",
              faker => faker.Random.Double(0, 100),
              faker => faker.Random.Double(0, 100));
  });
  ```

---

## Improvements
- **Streamlined Dictionary Handling**:  
  - Complex object values in dictionaries are created via `IApiMockDataFactory` automatically.  
  - Simplified configuration logic to reduce redundant code for nested objects.  

- **Enhanced Error Handling**:  
  - Improved error messages for invalid dictionary configurations or missing faker rules.  

---

## Unit Test Coverage
- **Comprehensive Tests**:  
  - Full test coverage for dictionaries with primitive and complex values.  
  - Validation of tuple generation with faker rules applied to both items.  
  - Mocking configurations tested for various property combinations, including nested lists, objects, and dictionaries.  

