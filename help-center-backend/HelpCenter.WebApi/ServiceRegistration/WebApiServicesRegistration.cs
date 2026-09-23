using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;
using HelpCenter.Application.Interfaces;
using HelpCenter.Persistence.Context;
using HelpCenter.Persistence.Services;
using HelpCenter.WebApi.Authorization;
using HelpCenter.WebApi.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace HelpCenter.WebApi.ServiceRegistration;

public static class WebApiServicesRegistration
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        var frontendUrl = configuration["AppSettings:FrontendUrl"];
        if (!Uri.TryCreate(frontendUrl, UriKind.Absolute, out var frontendUri) ||
            (frontendUri.Scheme != Uri.UriSchemeHttp && frontendUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "AppSettings:FrontendUrl is not configured with an absolute HTTP(S) URL. Set " +
                "AppSettings__FrontendUrl in the root .env — see .env.example.");
        }

        var allowedOrigins = new[]
            {
                "http://localhost:3000",
                "http://localhost:3001",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:3001",
                frontendUri.GetLeftPart(UriPartial.Authority)
            }
            .Where(origin => Uri.TryCreate(origin, UriKind.Absolute, out _))
            .Select(origin => origin!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowNextJs", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // JWT Authentication Configuration
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException(
                "JwtSettings:SecretKey is not configured. Set it via environment variable " +
                "(JwtSettings__SecretKey) in the root .env — see .env.example.");
        }

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/requestHub"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        // Swagger Configuration
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "HelpCenter API",
                Version = "v1",
                Description = "Hierarchical RBAC + ticket management platform."
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT token gir: Bearer {token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        services.AddScoped<IUserContext, HelpCenter.Persistence.Services.HttpBasedUserContext>();
        services.AddScoped<IRequestHubService, RequestHubService>();
        services.AddScoped<IDataScopeService, DataScopeService>();
        services.AddSignalR();

        // RBAC Authorization
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        // Rate Limiting (built-in .NET 8 API)
        // Partition key: "user_{id}" for authenticated users, otherwise the client IP.
        // This prevents users sharing a common IP behind NAT from punishing each other.
        //
        // Policies:
        //   - "auth"     → login     : 5 requests / 30 s  (brute-force protection)
        //   - "public"   → public    : 60 requests / 1 min (spam/DoS protection)
        //   - "password" → password  : 3 requests / 5 min (mail spam + code guessing)
        //   - "write"    → writes    : 30 requests / 1 min (message/ticket spam)
        //   - "upload"   → files     : 15 requests / 1 min (disk/bandwidth abuse)
        //   - "heavy"    → heavy     : 30 requests / 1 min (statistics/reports/logs)
        //   - "admin-write"   → admin mutations : 20 requests / 1 min per authenticated user
        //   - "rbac-mutation" → role/permission mutations : 10 requests / 1 min per authenticated user
        //   - "session-refresh" → refresh requests : 10 requests / 1 min per client IP
        //   - "session-logout"  → logout requests  : 20 requests / 1 min per client IP
        // Global fallback: 200 requests / 1 min (for anything not matched by a policy).
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, ct) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    var seconds = ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                    context.HttpContext.Response.Headers.RetryAfter = seconds;
                }

                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(
                    "{\"statusCode\":429,\"message\":\"Çok fazla istek gönderdiniz. Lütfen bir süre bekleyin.\"}",
                    ct);
            };

            static RateLimitPartition<string> Fixed(HttpContext ctx, int permit, TimeSpan window) =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(ctx),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permit,
                        Window = window,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });

            static RateLimitPartition<string> FixedByClientIp(HttpContext ctx, int permit, TimeSpan window) =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIpPartitionKey(ctx),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permit,
                        Window = window,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });

            options.AddPolicy("auth",     ctx => Fixed(ctx, 5,  TimeSpan.FromSeconds(30)));
            options.AddPolicy("public",   ctx => Fixed(ctx, 60, TimeSpan.FromMinutes(1)));
            options.AddPolicy("password", ctx => Fixed(ctx, 3,  TimeSpan.FromMinutes(5)));
            options.AddPolicy("write",    ctx => Fixed(ctx, 30, TimeSpan.FromMinutes(1)));
            options.AddPolicy("upload",   ctx => Fixed(ctx, 15, TimeSpan.FromMinutes(1)));
            options.AddPolicy("heavy",    ctx => Fixed(ctx, 30, TimeSpan.FromMinutes(1)));
            options.AddPolicy("admin-write", ctx => Fixed(ctx, 20, TimeSpan.FromMinutes(1)));
            options.AddPolicy("rbac-mutation", ctx => Fixed(ctx, 10, TimeSpan.FromMinutes(1)));
            options.AddPolicy("session-refresh", ctx => FixedByClientIp(ctx, 10, TimeSpan.FromMinutes(1)));
            options.AddPolicy("session-logout", ctx => FixedByClientIp(ctx, 20, TimeSpan.FromMinutes(1)));

            // Global fallback — also covers endpoints without attributes
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                Fixed(ctx, 200, TimeSpan.FromMinutes(1)));
        });

        return services;
    }

    /// <summary>
    /// Partition key for rate limiting: "user_{id}" if there is an authenticated user,
    /// otherwise the client IP (X-Forwarded-For or RemoteIpAddress).
    /// </summary>
    private static string GetPartitionKey(HttpContext context)
    {
        var userId = context.User?.FindFirst("UserId")?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
            return $"user_{userId}";

        return GetClientIpPartitionKey(context);
    }

    private static string GetClientIpPartitionKey(HttpContext context) =>
        $"ip_{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

    public static IApplicationBuilder UseWebApiConfiguration(this WebApplication app)
    {
        // Swagger stays off in Production — the CV/portfolio public demo intentionally hides
        // the full API surface from anonymous discovery. Enabled in every other environment.
        if (!app.Environment.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Static Files and Uploads Directory Configuration
        // ContentRootPath = the project root (HelpCenter.WebApi/). FileService uses the same
        // path — to avoid a path inconsistency with Assembly.Location (bin/Debug/...).
        var webApiRoot = app.Environment.ContentRootPath;

        var wwwrootPath = Path.Combine(webApiRoot, "wwwroot");
        if (!Directory.Exists(wwwrootPath))
        {
            Directory.CreateDirectory(wwwrootPath);
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(wwwrootPath)
        });

        var uploadsPath = Path.Combine(webApiRoot, "Uploads");
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsPath),
            RequestPath = "/Uploads"
        });

        return app;
    }
}
