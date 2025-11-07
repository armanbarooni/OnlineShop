using System.Text;

namespace OnlineShop.WebAPI.Middlewares
{
    public class RtlLocalizationMiddleware
    {
        private readonly RequestDelegate _next;

        public RtlLocalizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var isFa = IsFarsiRequest(context);

            if (!isFa)
            {
                await _next(context);
                return;
            }

            var originalBody = context.Response.Body;
            await using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            try
            {
                await _next(context);

                var contentType = context.Response.ContentType ?? string.Empty;
                memStream.Seek(0, SeekOrigin.Begin);

                if (contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
                {
                    using var reader = new StreamReader(memStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
                    var html = await reader.ReadToEndAsync();

                    var injection = GetHeadInjection();
                    var idx = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
                    string updated;
                    if (idx >= 0)
                    {
                        updated = html.Insert(idx, injection);
                    }
                    else
                    {
                        // No head found, prepend injection at the top
                        updated = injection + html;
                    }

                    var buffer = Encoding.UTF8.GetBytes(updated);
                    context.Response.ContentLength = buffer.Length;
                    context.Response.Body = originalBody;
                    await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                }
                else
                {
                    // Not HTML - just copy through
                    memStream.Seek(0, SeekOrigin.Begin);
                    await memStream.CopyToAsync(originalBody);
                }
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }

        private static bool IsFarsiRequest(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/fa", StringComparison.OrdinalIgnoreCase)) return true;

            var acceptLang = context.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrEmpty(acceptLang) && acceptLang.StartsWith("fa", StringComparison.OrdinalIgnoreCase)) return true;

            var query = context.Request.Query["lang"].ToString();
            if (!string.IsNullOrEmpty(query) && query.StartsWith("fa", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }

        private static string GetHeadInjection()
        {
            return @"<!-- RTL/Lang injection -->
<meta charset=""utf-8"" />
<meta http-equiv=""Content-Language"" content=""fa"" />
<link rel=""preload"" as=""font"" href=""/fa/assets/fonts/payda/PeydaWebFaNum-Regular.woff2"" type=""font/woff2"" crossorigin>
<link rel=""stylesheet"" href=""/css/rtl-global.css"" />";
        }
    }
}
