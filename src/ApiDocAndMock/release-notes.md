# Release 1.2.0 – Complex Type Support for Dictionary and Tuple

# Release 1.2.2 – 


### New: `CreateMockByType` Method  
- Introduced the `CreateMockByType` method, allowing for the **dynamic creation of MediatR commands and other types at runtime** without requiring generic type parameters (`<T>`).  
- This enhancement leverages **reflection** to invoke the existing `CreateMockObject<T>` method dynamically, ensuring that types are **fully instantiated and populated with mock data** even when the type is passed as a parameter (e.g., `typeof(CreateContactCommand)`).  
- The method simplifies testing and mocking scenarios by allowing dynamic generation of MediatR request objects, making it easier to **simulate workflows** and **populate complex types** without direct instantiation.  

---

###  Example Usage:  
```csharp
var mockCommand = _mockFactory.CreateMockByType(typeof(CreateContactCommand));

if (mockCommand is CreateContactCommand contact)
{
    Console.WriteLine($"Mocked Name: {contact.Name}");
    Console.WriteLine($"Mocked Email: {contact.Email}");
    Console.WriteLine($"Mocked Phone: {contact.Phone}");
}
```