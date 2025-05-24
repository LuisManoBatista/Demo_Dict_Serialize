using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleApp
{
    /// <summary>
    /// Custom JSON converter for Dictionary<string, object> that preserves the original data types
    /// including DateTime, boolean, numeric values, and nested collections.
    /// </summary>
    /// <remarks>
    /// This converter handles:
    /// - Primitive types (string, int, long, double, decimal, bool)
    /// - DateTime values (serialized in ISO 8601 format)
    /// - Nested dictionaries
    /// - Arrays and lists (including mixed-type arrays)
    /// - Nested objects
    /// 
    /// When deserializing, it attempts to determine the correct type for values
    /// based on the JSON structure and content.
    /// </remarks>
    public class ObjectDictionaryConverter : JsonConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"JsonTokenType.StartObject expected, but got {reader.TokenType}");
            }

            var dictionary = new Dictionary<string, object>();
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("JsonTokenType.PropertyName expected");
                }

                string key = reader.GetString() ?? string.Empty;
                reader.Read();
                
                dictionary.Add(key, ReadValue(ref reader));
            }
            
            return dictionary;
        }

        private object ReadValue(ref Utf8JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    string stringValue = reader.GetString() ?? string.Empty;
                    
                    // Try to parse the string as DateTime if it matches a date format
                    if (TryParseDateTime(stringValue, out DateTime dateTimeValue))
                    {
                        return dateTimeValue;
                    }
                    
                    return stringValue;
                    
                case JsonTokenType.Number:
                    if (reader.TryGetInt32(out int intValue))
                    {
                        return intValue;
                    }
                    if (reader.TryGetInt64(out long longValue))
                    {
                        return longValue;
                    }
                    if (reader.TryGetDouble(out double doubleValue))
                    {
                        return doubleValue;
                    }
                    return reader.GetDecimal();
                    
                case JsonTokenType.True:
                    return true;
                    
                case JsonTokenType.False:
                    return false;
                    
                case JsonTokenType.Null:
                    return null!;
                    
                case JsonTokenType.StartObject:
                    return ReadObject(ref reader);
                    
                case JsonTokenType.StartArray:
                    return ReadArray(ref reader);
                    
                default:
                    throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }
        }

        private List<object> ReadArray(ref Utf8JsonReader reader)
        {
            var list = new List<object>();
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return list;
                }
                
                // Handle different types within the array
                list.Add(ReadValue(ref reader));
            }
            
            throw new JsonException("Unexpected end of JSON array");
        }

        private Dictionary<string, object> ReadObject(ref Utf8JsonReader reader)
        {
            var nestedDictionary = new Dictionary<string, object>();
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return nestedDictionary;
                }
                
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("JsonTokenType.PropertyName expected");
                }
                
                string key = reader.GetString() ?? string.Empty;
                reader.Read();
                
                nestedDictionary.Add(key, ReadValue(ref reader));
            }
            
            throw new JsonException("Unexpected end of JSON object");
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            
            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key);
                WriteValue(writer, kvp.Value);
            }
            
            writer.WriteEndObject();
        }
        
        private void WriteValue(Utf8JsonWriter writer, object? value)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }
            
            switch (value)
            {
                case string stringValue:
                    writer.WriteStringValue(stringValue);
                    break;
                case int intValue:
                    writer.WriteNumberValue(intValue);
                    break;
                case long longValue:
                    writer.WriteNumberValue(longValue);
                    break;
                case double doubleValue:
                    writer.WriteNumberValue(doubleValue);
                    break;
                case decimal decimalValue:
                    writer.WriteNumberValue(decimalValue);
                    break;
                case bool boolValue:
                    writer.WriteBooleanValue(boolValue);
                    break;
                case DateTime dateTimeValue:
                    writer.WriteStringValue(dateTimeValue.ToString("o")); // ISO 8601 format
                    break;
                case Dictionary<string, object> dict:
                    writer.WriteStartObject();
                    foreach (var item in dict)
                    {
                        writer.WritePropertyName(item.Key);
                        WriteValue(writer, item.Value);
                    }
                    writer.WriteEndObject();
                    break;
                case IList<object> list:
                    writer.WriteStartArray();
                    foreach (var item in list)
                    {
                        WriteValue(writer, item);
                    }
                    writer.WriteEndArray();
                    break;
                case IEnumerable<object> enumerable:
                    writer.WriteStartArray();
                    foreach (var item in enumerable)
                    {
                        WriteValue(writer, item);
                    }
                    writer.WriteEndArray();
                    break;
                default:
                    if (value is object[] array)
                    {
                        writer.WriteStartArray();
                        foreach (var item in array)
                        {
                            WriteValue(writer, item);
                        }
                        writer.WriteEndArray();
                    }
                    else if (value is IEnumerable<object> enumerable)
                    {
                        writer.WriteStartArray();
                        foreach (var item in enumerable)
                        {
                            WriteValue(writer, item);
                        }
                        writer.WriteEndArray();
                    }
                    else
                    {
                        // Try to serialize unknown types as JSON or fall back to string
                        try
                        {
                            JsonSerializer.Serialize(writer, value);
                        }
                        catch
                        {
                            writer.WriteStringValue(value.ToString());
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Attempts to parse a string as a DateTime using multiple formats
        /// </summary>
        private bool TryParseDateTime(string value, out DateTime result)
        {
            // Try built-in parsing first (handles most common formats)
            if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out result))
            {
                return true;
            }
            
            // Define common date formats to try
            string[] dateFormats = {
                "yyyy-MM-ddTHH:mm:ss.fffffffZ",  // ISO 8601 with fractional seconds
                "yyyy-MM-ddTHH:mm:ssZ",          // ISO 8601 without fractional seconds
                "yyyy-MM-dd",                    // ISO 8601 date only
                "MM/dd/yyyy",                    // US date format
                "dd/MM/yyyy",                    // European date format
                "yyyy-MM-dd HH:mm:ss",           // ISO-like with space separator
                "yyyyMMddHHmmss",                // Compact format without separators
                "ddd, dd MMM yyyy HH:mm:ss 'GMT'", // RFC 1123 format
                "yyyy-MM-ddTHH:mm:ss.fffK"       // ISO 8601 with timezone
            };
            
            return DateTime.TryParseExact(value, dateFormats, null, 
                System.Globalization.DateTimeStyles.None, out result);
        }
    }
}
