using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Auth.API.Middlewares
{
    public class NonceInjectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;

        public NonceInjectionMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/")
            {
                // Generate a nonce value
                var nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", "").Replace("+", "").Replace("/", "");

                // Read the index.html file
                var indexPath = Path.Combine(_env.WebRootPath, "static-files/index.html");
                var content = await File.ReadAllTextAsync(indexPath);

                // Inject nonce into script and style tags
                content = content.Replace("<script", $"<script nonce=\"{nonce}\"")
                                 .Replace("<style", $"<style nonce=\"{nonce}\"");

                // Set the Content-Security-Policy header with nonce
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Xss-Protection", "1; mode=block");
                context.Response.Headers.Append("Referer-Policy", "no-referer");
                context.Response.Headers.Append("Content-Security-Policy",
                    $"default-src 'self' 'unsafe-inline';" +
                    $"script-src 'self' 'nonce-{nonce}'; " +
                    $"style-src 'self' https://fonts.googleapis.com 'unsafe-inline'; " +
                    $"font-src 'self' https://fonts.gstatic.com 'unsafe-inline'; " +
                    $"base-uri 'self'");

                // Serve the modified index.html content
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(content);
            }
            else
            {
                await _next(context);
            }
        }
    }

}
