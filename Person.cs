using System.Text.Json.Serialization;

namespace ConsoleApp;

public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    
    [JsonConverter(typeof(ObjectDictionaryConverter))]
    public Dictionary<string, object> Hobbies { get; set; } = new Dictionary<string, object>();

    public override string ToString()
    {
        return $"{Name}, {Age} years old, Hobbies: {string.Join(", ", Hobbies.Select(h => $"{h.Key}: {h.Value}"))}";
    }
}
