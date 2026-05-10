using BionicPRO.Configuration;
using BionicPRO.Middleware;
using BionicPRO.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace BionicPRO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            

            // Настройка Keycloak JWT аутентификации
            var keycloakOptions = builder.Configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>();
            

            // Загружаем JWKS один раз
            JsonWebKeySet jwks = null;
            try
            {
                var httpClient = new HttpClient();
                var jwksUrl    = $"{keycloakOptions?.GetAuthorityUrl()}/protocol/openid-connect/certs";
                Console.WriteLine($"Loading JWKS from: {jwksUrl}");

                var jwksResponse = httpClient.GetStringAsync(jwksUrl).GetAwaiter().GetResult();
                jwks = JsonSerializer.Deserialize<JsonWebKeySet>(jwksResponse);

                Console.WriteLine($"Loaded {jwks?.Keys?.Count ?? 0} keys from JWKS");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load JWKS: {ex.Message}");
                throw;
            }


            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority            = keycloakOptions?.GetAuthorityUrl();
                    options.RequireHttpsMetadata = keycloakOptions?.RequireHttpsMetadata ?? false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer   = true,
                        ValidIssuers = new []
                        {
                                "http://localhost:8080/realms/reports-realm", // для токенов из браузера
                                keycloakOptions?.GetAuthorityUrl()            // для внутренних вызовов
                        },
                        //ValidIssuer      = keycloakOptions?.GetAuthorityUrl(),
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew        = TimeSpan.FromMinutes(5),
                        // Используем предварительно загруженные ключи
                        IssuerSigningKeys = jwks.Keys,
                        //IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                        //{
                        //    // Загружаем JWKS при первом обращении
                        //    var client   = new HttpClient();
                        //    var jwksUrl  = $"{keycloakOptions?.GetAuthorityUrl()}/protocol/openid-connect/certs";
                        //    var response = client.GetStringAsync(jwksUrl).Result;
                        //    var keys     = JsonSerializer.Deserialize<JsonWebKeySet>(response);
                        //    return keys.Keys;
                        //},
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogError(context.Exception, "Authentication failed");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogInformation("Token validated successfully");
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("ProtheticUser", policy =>
                    policy.RequireClaim("realm_access", "prothetic_user"));

                options.AddPolicy("Administrator", policy =>
                    policy.RequireClaim("realm_access", "administrator"));
            });



            // Add services to the container.
            builder.Services.AddScoped<IClickHouseService, ClickHouseService>();
            builder.Services.AddScoped<IReportService, ReportService>();

            builder.Services.AddControllers();
            

            // Настройка CORS для React приложения
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });



            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseMiddleware<JwtValidationMiddleware>();
            app.UseCors("AllowReactApp");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Health check endpoint
            //app.MapGet("/", () => Results.Redirect("/swagger"));

            app.Run();
        }
    }
}
