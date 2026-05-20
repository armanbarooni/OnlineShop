using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace OnlineShop.WebAPI.Controllers
{
    /// <summary>
    /// Proxy controller for serving Mahak product images through our backend.
    /// This solves DNS/CORS issues when the client can't reach mahakacc.mahaksoft.com directly.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ImageProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ImageProxyController> _logger;
        private readonly IMemoryCache _cache;
        private const string MahakBaseUrl = "https://mahakacc.mahaksoft.com";

        public ImageProxyController(
            IHttpClientFactory httpClientFactory,
            ILogger<ImageProxyController> logger,
            IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Proxy a Mahak image URL through our server.
        /// Usage: /api/ImageProxy?url=/path/to/image.jpg
        /// </summary>
        [HttpGet]
        [ResponseCache(Duration = 86400)] // Cache for 24 hours at HTTP level
        public async Task<IActionResult> GetImage([FromQuery] string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("URL is required");
            }

            // Security: only allow Mahak URLs
            string fullUrl;
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                if (!url.Contains("mahaksoft.com", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Only Mahak image URLs are allowed");
                }
                fullUrl = url;
            }
            else
            {
                // Relative URL - prepend Mahak base
                fullUrl = $"{MahakBaseUrl}{(url.StartsWith("/") ? "" : "/")}{url}";
            }

            // Check memory cache first
            var cacheKey = $"img_proxy_{fullUrl.GetHashCode()}";
            if (_cache.TryGetValue(cacheKey, out (byte[] Data, string ContentType) cached))
            {
                return File(cached.Data, cached.ContentType);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ImageProxy");
                client.Timeout = TimeSpan.FromSeconds(15);

                var response = await client.GetAsync(fullUrl, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch image from Mahak: {Url}, Status: {Status}", 
                        fullUrl, response.StatusCode);
                    return NotFound("Image not found");
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

                // Cache in memory for 1 hour
                _cache.Set(cacheKey, (imageBytes, contentType), TimeSpan.FromHours(1));

                return File(imageBytes, contentType);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to proxy image from Mahak: {Url}", fullUrl);
                
                // Return nophoto fallback
                var nophotoPath = Path.Combine(
                    Directory.GetCurrentDirectory(), 
                    "wwwroot", "fa", "assets", "images", "product", "nophoto.png");
                
                if (System.IO.File.Exists(nophotoPath))
                {
                    var fallbackBytes = await System.IO.File.ReadAllBytesAsync(nophotoPath, cancellationToken);
                    return File(fallbackBytes, "image/png");
                }

                return NotFound("Image unavailable");
            }
        }
    }
}
