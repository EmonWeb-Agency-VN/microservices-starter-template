using Auth.API.Middlewares;
using Auth.API.StartupTasks;
using Auth.API.Swagger;
using Auth.Service;
using Common.Application.Behaviours;
using Common.Application.Implementations;
using Common.Application.Infrastructure.Time;
using Common.Application.Time;
using Common.Authorization.AuthorizationHandlers;
using Common.Domain.Configurations;
using Common.Domain.Interfaces;
using Common.Persistence.Core;
using Common.Persistence.InitDataHelper;
using Common.Persistence.Interceptors;
using Common.Persistence.ServiceInstallers;
using Common.Proxies.Authentication;
using Common.Proxies.Extensions;
using Common.Proxies.Invokers;
using Common.SharedKernel;
using Common.SharedKernel.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Extensions.Logging;
using NLog.Targets.Wrappers;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;

namespace Auth.API
{
    public class Program
    {
        private static readonly CommonConfig _commonConfig = new CommonConfig();
        public static async Task Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                var builder = WebApplication.CreateBuilder(args);

#pragma warning disable ASP0013 // Suggest switching from using Configure methods to WebApplicationBuilder.Configuration
                builder.WebHost
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        if (context.HostingEnvironment.IsDevelopment())
                        {
                            string folder = AppDomain.CurrentDomain.BaseDirectory;
                            string configFilePath = Path.Combine(folder, "appsettings.json");
                            if (File.Exists(configFilePath))
                            {
                                config.AddJsonFile(configFilePath, optional: true, reloadOnChange: true);
                            }
                        }
                        config.AddEnvironmentVariables();

                        var commonConfig = config.Build();
                        commonConfig.Bind(_commonConfig);
                    })
                    .ConfigureKestrel(options =>
                    {
                        var port = builder.Environment.IsDevelopment() ? 49671 : 443;
                        options.ListenAnyIP(port, listenOptions =>
                        {
                            listenOptions.UseHttps(_commonConfig.SSLCertificate.Path);
                        });
                    });
#pragma warning restore ASP0013 // Suggest switching from using Configure methods to WebApplicationBuilder.Configuration
                builder.Host.AddLoggingConfiguration(_commonConfig);
                ConfigureServices(builder);
                await ConfigureApp(builder);

            }
            catch (Exception ex) when (ex is not HostAbortedException && ex.Source != "Microsoft.EntityFrameworkCore.Design")
            {
                logger.Fatal(ex, $"ERROR: {ex.Message}");
                throw;
            }
            finally
            {
                if (LogManager.Configuration != null && LogManager.Configuration.AllTargets.OfType<BufferingTargetWrapper>().Any())
                {
                    LogManager.Flush();
                }
                LogManager.Shutdown();
            }

        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;
            services.AddSingleton<ILoggerProvider, NLogLoggerProvider>();
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            #region Common
            services.AddSharedInfrastructure();
            services.AddEndpointsApiExplorer();
            services.AddHttpContextAccessor();
            services.AddCors(options =>
            {
                options.AddPolicy(name: "auth_cors_policy", policy =>
                {
                    policy.WithOrigins("https://localhost:5173").AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithExposedHeaders("IS-TOKEN-EXPIRED");
                });
            });
            #endregion

            #region Swagger
            services.ConfigureOptions<SwaggerGenOptionsSetup>();
            services.ConfigureOptions<SwaggerUiOptionsSetup>();
            services.AddSwaggerGen();
            #endregion

            #region Authentication
            services.Configure<AuthenticationOptions>(configuration.GetSection(nameof(AuthenticationOptions)));
            services.AddAuthentication()
                 .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                 {
                     options.Cookie.Name = Constants.DefaultCookieName;
                     options.Cookie.HttpOnly = true;
                     //options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                     options.SlidingExpiration = true;
                     options.Cookie.SameSite = SameSiteMode.None;
                     options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                     options.EventsType = typeof(CustomCookieAuthenticationEvents);
                 })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _commonConfig.AuthenticationOptions.JwtBearer.ValidIssuer,
                        ValidAudience = _commonConfig.AuthenticationOptions.JwtBearer.ValidAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_commonConfig.AuthenticationOptions.JwtBearer.SecretKey)),
                        ClockSkew = TimeSpan.FromSeconds(_commonConfig.AuthenticationOptions.JwtBearer.ClockSkew)
                    };
                    options.MapInboundClaims = false;
                    options.EventsType = typeof(CustomJwtAuthenticationEvents);
                });
            services.AddScoped<CustomCookieAuthenticationEvents>();
            services.AddScoped<CustomJwtAuthenticationEvents>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserSessionService, UserSessionService>();
            services.AddScoped<IUserService, UserService>();
            #endregion

            #region Authorization
            services.AddAuthorization(options =>
            {
                var appPolicyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
                options.AddPolicy("App", appPolicyBuilder
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .Build());

                var webPolicyBuilder = new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme);
                options.AddPolicy("Web", webPolicyBuilder
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
                    .Build());
            });
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
            #endregion

            RegisterDependencies(services, configuration);
        }

        private static void RegisterDependencies(IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddMemoryCache();
            services.AddScoped<ICustomMemoryCacheService, CustomMemoryCacheService>();

            #region Rate limit
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy(Constants.RateLimiterForAnonymous, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Request.Headers["X-Forwarded-For"].ToString(),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromSeconds(60),
                            PermitLimit = 5
                        }
                    ));
                options.AddPolicy(Constants.RateLimiterForDefault, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.CurrentUserId(),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromSeconds(60),
                            PermitLimit = 100
                        }
                    ));
            });
            #endregion

            #region Audit
            //services.AddScoped<IAuditService, AuditService>();
            #endregion

            #region System configuration
            services.AddTransient<ISystemTime, SystemTime>();
            #endregion

            #region DBContext
            services.AddScoped<IDBDataService, DBDataService>();
            // Register all services implement target interfaces as scoped in target assembly
            services.AddScopedAsMatchingInterfaces(Common.Persistence.AssemblyReference.Assembly);
            services.AddSingleton<UpdateAuditableEntitiesInterceptor>();
            services.AddScoped<DbContext, UserDbContext>();
            services.AddDbContext<UserDbContext>((serviceProvider, options) =>
            {
                var connectionString = _commonConfig.ConnectionStrings.DbConnection;
                options.UseNpgsql(connectionString)
                        .AddInterceptors(serviceProvider.GetService<UpdateAuditableEntitiesInterceptor>()!);
            });
            services.AddScoped<IDBRepository, DBRepository>();
            services.AddHostedService<MigrateDatabaseStartupTask>();
            #endregion

            #region MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AssemblyReference.Assembly));
            //services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
            ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
            ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Stop;

            #endregion

            services.AddHttpClient();
            services.AddSingleton<ServiceInvoker>();
        }

        private static async Task ConfigureApp(WebApplicationBuilder builder)
        {
            var app = builder.Build();

            app.UseForwardedHeaders();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<GlobalExceptionHandlingMiddleware>(); // Register global exception handling middleware first

            app.UseRouting();
            app.UseCors("auth_cors_policy");
            app.UseHttpsRedirection();
            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseJwtTokenMiddleware();

            app.UseMiddleware<NonceInjectionMiddleware>();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    if (ctx.Context.Request.Path.HasValue)
                    {
                        if (!ctx.Context.Request.Path.Value.Contains("/static-files") && !ctx.Context.User.Identity.IsAuthenticated)
                        {
                            ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            ctx.Context.Response.ContentLength = 0;
                            ctx.Context.Response.Body = Stream.Null;
                            ctx.Context.Response.Headers.Append("Cache-Control", "no-store");
                        }
                    }
                }
            });
            app.UseAuthorization();
            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                await next(context);
            });
            app.UseMiddleware<RequestTimingMiddleware>();

            app.MapControllers();

            app.MapFallbackToFile("static-files/index.html")
                .RequireRateLimiting(_commonConfig.RateLimiter.Policy);

            await app.RunAsync();
        }
    }
}

