// This is a C# console application demonstrating JSON serialization and deserialization
// It shows how to convert objects to JSON and vice versa

using System.Text.Json;
using ConsoleApp;

// Top level statements (C# 9.0 and later feature)
Console.WriteLine("JSON Serialization and Deserialization Demo");
Console.WriteLine("------------------------------------------");

// Create a Person object
var person = new Person
{
    Name = "John Doe",
    Age = 30,
    Hobbies = new Dictionary<string, object> 
    { 
        { "Reading", "Fiction novels" },
        { "Hiking", new[] { "Mountains", "Forests", "Beaches" } },
        { "Programming", new Dictionary<string, object> { { "Languages", new[] { "C#", "JavaScript", "Python" } }, { "Experience", 5 } } },
        { "Joined", DateTime.Now },
        { "IsActive", true },
        { "LastLogin", DateTime.Now.AddDays(-3) },
        { "IsPremium", false }
    }
};

// 1. Serialize object to JSON string
Console.WriteLine("\n1. Serializing object to JSON:");
var options = new JsonSerializerOptions { WriteIndented = true }; // For pretty printing
string jsonString = JsonSerializer.Serialize(person, options);
Console.WriteLine(jsonString);

// 2. Deserialize JSON string back to object
Console.WriteLine("\n2. Deserializing JSON back to object:");
Person? deserializedPerson = JsonSerializer.Deserialize<Person>(jsonString);
Console.WriteLine($"Deserialized person: {deserializedPerson}");

// Debug the types in the dictionary
Console.WriteLine("\nDebug - Types in deserialized dictionary:");
foreach (var hobby in deserializedPerson?.Hobbies ?? new Dictionary<string, object>())
{
    Console.WriteLine($"Key: {hobby.Key}, Value: {hobby.Value}, Type: {hobby.Value?.GetType().Name}");
    
    // If it's a list, show inner types
    if (hobby.Value is List<object> list)
    {
        Console.WriteLine($"  List contents: {string.Join(", ", list)}");
        Console.WriteLine($"  List item types: {string.Join(", ", list.Select(item => item?.GetType().Name ?? "null"))}");
    }
    // If it's a nested dictionary, show inner key-values
    else if (hobby.Value is Dictionary<string, object> nestedDict)
    {
        Console.WriteLine($"  Dictionary contents:");
        foreach (var item in nestedDict)
        {
            Console.WriteLine($"    {item.Key}: {item.Value} (Type: {item.Value?.GetType().Name ?? "null"})");
        }
    }
    // If it's a DateTime, show formatted value
    else if (hobby.Value is DateTime dateTime)
    {
        Console.WriteLine($"  DateTime formatted: {dateTime:yyyy-MM-dd HH:mm:ss}");
    }
    // If it's a boolean, show string representation
    else if (hobby.Value is bool boolean)
    {
        Console.WriteLine($"  Boolean value: {boolean.ToString().ToLower()}");
    }
}

// 3. Save JSON to file
string fileName = "person.json";
Console.WriteLine($"\n3. Saving JSON to file '{fileName}':");
File.WriteAllText(fileName, jsonString);
Console.WriteLine($"JSON saved to {Path.GetFullPath(fileName)}");

// 4. Read JSON from file and deserialize
Console.WriteLine($"\n4. Reading JSON from file and deserializing:");
string jsonFromFile = File.ReadAllText(fileName);
Person? personFromFile = JsonSerializer.Deserialize<Person>(jsonFromFile);
Console.WriteLine($"Person from file: {personFromFile}");

// 5. Interactive JSON creation
Console.WriteLine("\n5. Create your own person object to serialize:");
var customPerson = new Person();

Console.Write("Enter name: ");
customPerson.Name = Console.ReadLine() ?? "Anonymous";

Console.Write("Enter age: ");
if (int.TryParse(Console.ReadLine(), out int age))
{
    customPerson.Age = age;
}

Console.WriteLine("Enter hobbies (format: key=value, key=value):");
Console.WriteLine("Example: Reading=Fiction, Sports=Basketball, IsActive=true, JoinDate=2023-01-15");
string? hobbiesInput = Console.ReadLine();
if (!string.IsNullOrWhiteSpace(hobbiesInput))
{
    var hobbiesEntries = hobbiesInput.Split(',');
    foreach (var entry in hobbiesEntries)
    {
        var keyValue = entry.Split('=');
        if (keyValue.Length == 2)
        {
            string key = keyValue[0].Trim();
            string valueStr = keyValue[1].Trim();
            
            // Try to parse different value types
            if (bool.TryParse(valueStr, out bool boolValue))
            {
                customPerson.Hobbies[key] = boolValue;
            }
            else if (DateTime.TryParse(valueStr, out DateTime dateValue))
            {
                customPerson.Hobbies[key] = dateValue;
            }
            else if (int.TryParse(valueStr, out int intValue))
            {
                customPerson.Hobbies[key] = intValue;
            }
            else
            {
                customPerson.Hobbies[key] = valueStr;
            }
        }
    }
}

// Serialize the custom person
Console.WriteLine("\nYour person object as JSON:");
Console.WriteLine(JsonSerializer.Serialize(customPerson, options));

Console.WriteLine("\n6. Testing DateTime serialization specifically:");
var dateTimeTest = new Dictionary<string, object>
{
    { "Created", DateTime.Now },
    { "Modified", DateTime.UtcNow },
    { "SpecificDate", new DateTime(2025, 5, 24) },
    { "WithTime", new DateTime(2025, 5, 24, 13, 45, 30) }
};

var person2 = new Person
{
    Name = "Date Test Person",
    Age = 25,
    Hobbies = dateTimeTest
};

string dateTimeJson = JsonSerializer.Serialize(person2, options);
Console.WriteLine(dateTimeJson);

Console.WriteLine("\nDeserializing DateTime test:");
var deserializedDateTimePerson = JsonSerializer.Deserialize<Person>(dateTimeJson);

if (deserializedDateTimePerson != null)
{
    Console.WriteLine("\nVerifying DateTime types in deserialized object:");
    foreach (var item in deserializedDateTimePerson.Hobbies)
    {
        if (item.Value is DateTime dt)
        {
            Console.WriteLine($"{item.Key}: {dt:yyyy-MM-dd HH:mm:ss} (Type: DateTime)");
        }
        else
        {
            Console.WriteLine($"{item.Key}: {item.Value} (Type: {item.Value?.GetType().Name})");
        }
    }
}

// 7. Testing nested arrays with mixed types
Console.WriteLine("\n7. Testing nested arrays with mixed types:");
var mixedArrayTest = new Dictionary<string, object>
{
    { "SimpleArray", new[] { "string1", "string2", "string3" } },
    { "MixedArray", new object[] { "string", 123, true, DateTime.Now } },
    { "NestedArray", new object[] { 
        new object[] { 1, 2, 3 }, 
        new Dictionary<string, object> { { "Key", "Value" } },
        "Simple string"
    } }
};

var person3 = new Person
{
    Name = "Array Test Person",
    Age = 35,
    Hobbies = mixedArrayTest
};

string arrayJson = JsonSerializer.Serialize(person3, options);
Console.WriteLine(arrayJson);

Console.WriteLine("\nDeserializing array test:");
var deserializedArrayPerson = JsonSerializer.Deserialize<Person>(arrayJson);

if (deserializedArrayPerson != null)
{
    Console.WriteLine("\nVerifying array types in deserialized object:");
    foreach (var item in deserializedArrayPerson.Hobbies)
    {
        Console.WriteLine($"{item.Key}: (Type: {item.Value?.GetType().Name})");
        
        if (item.Value is List<object> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Console.WriteLine($"  [{i}]: {list[i]} (Type: {list[i]?.GetType().Name})");
                
                // If it's a nested list
                if (list[i] is List<object> nestedList)
                {
                    for (int j = 0; j < nestedList.Count; j++)
                    {
                        Console.WriteLine($"    [{j}]: {nestedList[j]} (Type: {nestedList[j]?.GetType().Name})");
                    }
                }
                else if (list[i] is Dictionary<string, object> nestedDict)
                {
                    foreach (var kvp in nestedDict)
                    {
                        Console.WriteLine($"    {kvp.Key}: {kvp.Value} (Type: {kvp.Value?.GetType().Name})");
                    }
                }
            }
        }
    }
}

Console.WriteLine("\nPress any key to exit...");
try
{
    Console.ReadLine();
}
catch
{
    // In non-interactive environments, just exit gracefully
}
