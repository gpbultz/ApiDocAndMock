# Release Notes - Version 1.1.0

## 📦 New in This Release

---

### 🔧 Dependency Injection Refactor

#### Overview:

- Refactored static classes to support **Dependency Injection (DI)**.
- This simplifies testing, improves maintainability, and aligns with modern best practices.

#### Key Changes:

- **`ApiMockDataFactory`** and **`MockConfigurationsFactory`** are now injectable via DI.
- The need for static wrappers has been removed, allowing seamless integration.
- `AddMockingConfigurations` now registers services directly within the DI container.
- Example:
    
    ```csharp
    builder.Services.AddMockingConfigurations(config =>
    {
        config.RegisterConfiguration<Booking>(cfg =>
        {
            cfg.ForPropertyObject<Room>("Room");
        });
    });
    ```
    

#### Impact:

- Backwards compatibility is maintained through transitional wrappers.
- Developers can now directly inject dependencies into endpoint handlers, controllers, or services.

---

### 🐞 Bug Fixes - Memory DB

#### Overview:

- Addressed issues where type mismatches during lookups caused failures in `GetByField`, `Update`, and `Delete` operations.
- Resolved cases where GUIDs passed as strings could not match stored GUID values.

#### Fixes:

1. **Type Conversion**:
    
    - `GetByField`, `Update`, and `Delete` now dynamically convert `string` GUIDs to `Guid` before comparison.
    
    ```csharp
    if (property.PropertyType == typeof(Guid) && value is string stringValue)
    {
        Guid.TryParse(stringValue, out var guidValue);
        convertedValue = guidValue;
    }
    ```
    
2. **Path Parameter Case Sensitivity**:
    
    - Fixed an issue where Swagger/OpenAPI generated duplicate path parameters (`id` vs `Id`).
    - Case-insensitive checks now prevent duplication.
    
    ```csharp
    if (!operation.Parameters.Any(p =>
        string.Equals(p.Name, sourceIdFieldName, StringComparison.OrdinalIgnoreCase) &&
        p.In == ParameterLocation.Path))
    ```
    
3. **Empty Store Issue**:
    
    - Addressed cases where `_store` contained a key but with an empty list (`Count = 0`), causing unexpected lookup failures.
    - Now properly verifies if the item exists before deletion.

#### Testing & Verification:

- Unit tests have been updated to cover scenarios where `Guid`, `int`, and `string` types are passed in.
- Verified that the changes do not impact existing workflows.

---

### ⚙️ How to Upgrade

1. Update your NuGet package to version `1.1.0`:
    
    ```bash
    dotnet add package ApiDocAndMock --version 1.1.0
    ```
    
2. Modify your service registrations to use the new DI patterns.
3. Test existing workflows to ensure type conversions work as expected.
