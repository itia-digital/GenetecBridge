using System.Text.Json;

namespace Core.Data.Extensions;

public static class JsonExtension
{
    public static string ToJson(this object o, JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(o, options);
    
    public static T? FromJson<T>(this string text, JsonSerializerOptions? options = null)
        => JsonSerializer.Deserialize<T>(text, options);
}