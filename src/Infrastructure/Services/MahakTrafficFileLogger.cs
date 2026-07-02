using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OnlineShop.Infrastructure.Services
{
    public class MahakTrafficFileLogger : IMahakTrafficLogger
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly IConfiguration _configuration;
        private readonly ILogger<MahakTrafficFileLogger> _logger;

        public MahakTrafficFileLogger(
            IConfiguration configuration,
            ILogger<MahakTrafficFileLogger> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task LogRequestAsync(
            string operation,
            string endpoint,
            string comment,
            object? payload,
            CancellationToken cancellationToken)
        {
            return WriteAsync(operation, endpoint, "Request", comment, null, null, payload, cancellationToken);
        }

        public Task LogResponseAsync(
            string operation,
            string endpoint,
            string comment,
            int statusCode,
            bool isSuccessStatusCode,
            string? payload,
            CancellationToken cancellationToken)
        {
            return WriteAsync(operation, endpoint, "Response", comment, statusCode, isSuccessStatusCode, payload, cancellationToken);
        }

        private async Task WriteAsync(
            string operation,
            string endpoint,
            string direction,
            string comment,
            int? statusCode,
            bool? isSuccessStatusCode,
            object? payload,
            CancellationToken cancellationToken)
        {
            try
            {
                var logDirectory = ResolveLogDirectory();
                Directory.CreateDirectory(logDirectory);

                var timestamp = DateTimeOffset.Now;
                var fileName = $"{timestamp:yyyyMMdd-HHmmss-fff}-{Sanitize(operation)}-{direction.ToLowerInvariant()}.json";
                var filePath = Path.Combine(logDirectory, fileName);

                var logEntry = new
                {
                    timestamp,
                    operation,
                    endpoint,
                    direction,
                    comment,
                    statusCode,
                    isSuccessStatusCode,
                    data = NormalizePayload(payload)
                };

                await File.WriteAllTextAsync(
                    filePath,
                    JsonSerializer.Serialize(logEntry, JsonOptions),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to write Mahak traffic log for {Operation} {Direction}", operation, direction);
            }
        }

        private string ResolveLogDirectory()
        {
            var configuredPath = _configuration["Mahak:TrafficLogPath"];
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                return Path.GetFullPath(configuredPath);
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "MahakTrafficLogs");
        }

        private static object? NormalizePayload(object? payload)
        {
            if (payload == null)
            {
                return null;
            }

            if (payload is string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }

                try
                {
                    return JsonSerializer.Deserialize<JsonElement>(text);
                }
                catch (JsonException)
                {
                    return text;
                }
            }

            return payload;
        }

        private static string Sanitize(string value)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '-');
            }

            return value.Replace(' ', '-').ToLowerInvariant();
        }
    }
}
