# C# Console Application - JSON Serialization Demo

A C# console application demonstrating advanced JSON serialization and deserialization with custom type preservation.

## Features

- Serializes and deserializes objects to/from JSON
- Preserves types in `Dictionary<string, object>` collections
- Handles special types like `DateTime` and `bool` correctly
- Supports nested objects, arrays, and dictionaries
- Interactive creation of objects to serialize

## Custom Type Handling

This application includes a custom `ObjectDictionaryConverter` that properly handles various data types:

- Primitive types (string, int, double, bool)
- DateTime values (serialized in ISO 8601 format)
- Nested dictionaries and arrays
- Mixed-type collections

## Project Structure

- `Program.cs`: Main entry point with demonstration code
- `Person.cs`: Sample class with a dictionary property for JSON serialization
- `ObjectDictionaryConverter.cs`: Custom JSON converter for type preservation
- `ConsoleApp.csproj`: Project file containing build configurations

## Running the Application

You can run the application using the following command:

```bash
dotnet run
```

## Building the Application

To build the application, use:

```bash
dotnet build
```

## Implementation Details

The key to preserving types is the custom `JsonConverter` implementation that:

1. During serialization:
   - Handles each value type specifically (string, number, boolean, DateTime)
   - Formats DateTime values using ISO 8601
   - Recursively processes nested dictionaries and arrays

2. During deserialization:
   - Detects value types from JSON tokens
   - Attempts to parse strings as DateTime when they match date formats
   - Correctly reconstructs nested structures

## Example Usage

```csharp
var data = new Dictionary<string, object> 
{ 
    { "Text", "Sample text" },
    { "Number", 123 },
    { "IsValid", true },
    { "Created", DateTime.Now },
    { "Items", new[] { "Item1", "Item2", "Item3" } }
};

// Serialize
string json = JsonSerializer.Serialize(data);

// Deserialize
var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
```
