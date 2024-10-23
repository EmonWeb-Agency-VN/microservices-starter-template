using Common.SharedKernel;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Auth.API.Swagger;

/// <summary>
/// Represents the <see cref="SwaggerGenOptions"/> setup.
/// </summary>
internal sealed class SwaggerGenOptionsSetup : IConfigureOptions<SwaggerGenOptions>
{
    /// <inheritdoc />
    public void Configure(SwaggerGenOptions options)
    {
        options.EnableAnnotations();

        options.SwaggerDoc("Web", new OpenApiInfo
        {
            Version = "0.0.1",
            Title = "Auth WEB API",
            Description = "This swagger document describes the available API endpoints for Web."
        });

        //options.AddSecurityDefinition(
        //    JwtBearerDefaults.AuthenticationScheme,
        //    new OpenApiSecurityScheme
        //    {
        //        Name = HeaderNames.Authorization,
        //        In = ParameterLocation.Header,
        //        Type = SecuritySchemeType.Http,
        //        Scheme = JwtBearerDefaults.AuthenticationScheme,
        //        Description = "JWT Authentication using the Bearer scheme."
        //    });

        options.AddSecurityDefinition(
            "SessionId",
            new OpenApiSecurityScheme
            {
                Name = Constants.DefaultSessionId,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                //Scheme = JwtBearerDefaults.AuthenticationScheme,
                //Description = "JWT Authentication using the Bearer scheme."
            });

        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "SessionId"
                        }
                    },
                    Array.Empty<string>()
                }
            });

        options.CustomSchemaIds(type => type.FullName);

        options.OperationFilter<AuthorizeOperationFilter>();
    }
}
