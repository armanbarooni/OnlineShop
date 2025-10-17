using System.Text.Json;

namespace OnlineShop.IntegrationTests.Helpers
{
    public static class JsonHelper
    {
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
                    if (!element.TryGetProperty(property, out element))
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
                    if (!element.TryGetProperty(property, out element))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

