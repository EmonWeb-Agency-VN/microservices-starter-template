using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Auth.API.Swagger;

/// <summary>
/// Represents the <see cref="SwaggerUIOptions"/> setup.
/// </summary>
internal sealed class SwaggerUiOptionsSetup : IConfigureOptions<SwaggerUIOptions>
{
    /// <inheritdoc />
    public void Configure(SwaggerUIOptions options)
    {
        options.DisplayRequestDuration();
        options.SwaggerEndpoint("Web/swagger.json", "Web");
    }
}
