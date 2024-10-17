using AuthService.Application.Auth.Commands;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.FeatureManagement;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using System.Reflection;

namespace AuthService.Api
{
    public static class Program
    {
        private static readonly string[] EmptyStringArray = Array.Empty<string>();

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder);

            var app = builder.Build();

            ConfigureApp(app);

            try
            {
                Log.Information("Starting web host");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.ApplicationInsights(TelemetryConfiguration.CreateDefault(), TelemetryConverter.Traces)
                .CreateLogger();

            builder.Host.UseSerilog();

            // Load configuration from Azure App Configuration
            ConfigureAzureAppConfiguration(builder);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Configure Swagger/OpenAPI
            ConfigureSwagger(builder);

            // Configure DbContext
            builder.Services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configure MediatR
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SignUpCommand).Assembly));

            // Configure services
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();

            // Configure JWT authentication
            ConfigureJwtAuthentication(builder);

            // Add Feature Management
            builder.Services.AddFeatureManagement();

            // Add Application Insights
            builder.Services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
            });
        }

        private static void ConfigureApp(WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("./swagger/v1/swagger.json", "Auth Service API v1");
                    c.RoutePrefix = string.Empty; // To serve the Swagger UI at the app's root
                });
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseSerilogRequestLogging();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
        }

        private static void ConfigureAzureAppConfiguration(WebApplicationBuilder builder)
        {
            try
            {
                string appConfigConnectionString = builder.Configuration["AzureAppConfiguration:ConnectionString"] ?? string.Empty;
                if (!string.IsNullOrEmpty(appConfigConnectionString))
                {
                    builder.Configuration.AddAzureAppConfiguration(options =>
                    {
                        options.Connect(appConfigConnectionString)
                               .UseFeatureFlags()
                               .ConfigureKeyVault(kv =>
                               {
                                   kv.SetCredential(new DefaultAzureCredential(new DefaultAzureCredentialOptions
                                   {
                                       ExcludeEnvironmentCredential = true,
                                       ExcludeManagedIdentityCredential = true,
                                       ExcludeSharedTokenCacheCredential = true,
                                       ExcludeVisualStudioCredential = true,
                                       ExcludeVisualStudioCodeCredential = true,
                                       ExcludeAzureCliCredential = false,
                                       ExcludeInteractiveBrowserCredential = true
                                   }));
                               });
                    });
                }
                else
                {
                    Log.Warning("Azure App Configuration connection string is not provided. Skipping Azure App Configuration setup.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while configuring Azure App Configuration.");
            }
        }

        private static void ConfigureSwagger(WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Auth Service API",
                    Version = "v1",
                    Description = "API for user authentication and authorization",
                    Contact = new OpenApiContact
                    {
                        Name = "API Support",
                        Email = "support@example.com",
                        Url = new Uri("https://www.example.com/support")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Apache 2.0",
                        Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
                else
                {
                    Log.Warning("XML documentation file not found. Swagger comments will not be included.");
                }

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
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
                        EmptyStringArray
                    }
                });
            });
        }

        private static void ConfigureJwtAuthentication(WebApplicationBuilder builder)
        {
            var jwtSecret = builder.Configuration["JWT:Secret"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                Log.Error("JWT:Secret is not configured.");
                throw new InvalidOperationException("JWT:Secret is not configured.");
            }

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JWT:Issuer"],
                        ValidAudience = builder.Configuration["JWT:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                    };
                });
        }
    }
}