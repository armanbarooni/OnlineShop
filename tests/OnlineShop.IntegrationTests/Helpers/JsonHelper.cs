using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnlineShop.IntegrationTests.Helpers
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string? GetNestedProperty(string jsonContent, params string[] propertyPath)
        {
            if (string.IsNullOrEmpty(jsonContent))
                return null;

            try
            {
                using var document = JsonDocument.Parse(jsonContent);
                var element = document.RootElement;

                foreach (var property in propertyPath)
                {
                    // Try exact match first
                    if (element.TryGetProperty(property, out var exactMatch))
                    {
                        element = exactMatch;
                        continue;
                    }

                    // Try case-insensitive match
                    var found = false;
                    foreach (var prop in element.EnumerateObject())
                    {
                        if (string.Equals(prop.Name, property, StringComparison.OrdinalIgnoreCase))
                        {
                            element = prop.Value;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        return null;
                }

                return element.ValueKind == JsonValueKind.String 
                    ? element.GetString() 
                    : element.GetRawText();
            }
            catch
            {
                return null;
            }
        }

        public static bool TryGetNestedProperty(string jsonContent, out JsonElement element, params string[] propertyPath)
        {
            element = default;
            
            if (string.IsNullOrEmpty(jsonContent))
                return false;

            try
            {
                using var document = JsonDocument.Parse(jsonContent);
                element = document.RootElement;

                foreach (var property in propertyPath)
                {
                    // Try exact match first
                    if (element.TryGetProperty(property, out var exactMatch))
                    {
                        element = exactMatch;
                        continue;
                    }

                    // Try case-insensitive match
                    var found = false;
                    foreach (var prop in element.EnumerateObject())
                    {
                        if (string.Equals(prop.Name, property, StringComparison.OrdinalIgnoreCase))
                        {
                            element = prop.Value;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deserialize JSON directly to AuthResponseDto with case-insensitive matching
        /// </summary>
        public static T? Deserialize<T>(string jsonContent) where T : class
        {
            if (string.IsNullOrEmpty(jsonContent))
                return null;

            try
            {
                return JsonSerializer.Deserialize<T>(jsonContent, JsonOptions);
            }
            catch
            {
                return null;
            }
        }
    }
}

