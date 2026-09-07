using System;
using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using GameSense.Api.Mapping;
using GameSense.Api.Validators;
using GameSense.Api.Behaviors;
using GameSense.Core.Repositories;
using GameSense.Core.Services;
using GameSense.Infrastructure.Data;
using GameSense.Infrastructure.Repositories;
using GameSense.Infrastructure.Services;
using GameSense.Infrastructure.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GameSense.Core.Models;
using Microsoft.AspNetCore.Identity;
using GameSense.Api.Services;

namespace GameSense.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGameSenseServices(this IServiceCollection services, IConfiguration configuration)
        {
            // FluentValidation
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<CreateReviewRequestValidator>();

            // MediatR (scan this assembly for handlers)
            services.AddMediatR(typeof(ServiceCollectionExtensions).Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // AutoMapper (scan this assembly for profiles)
            services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);

            // DbContext
            var defaultConn = configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=GameSenseDb;Trusted_Connection=True;";
            services.AddDbContext<GameSenseDbContext>(opts => opts.UseSqlServer(defaultConn));

            services.Configure<GameCatalogOptions>(configuration.GetSection("GameCatalog"));
            services.AddHttpClient("GameCatalogApi", (provider, client) =>
            {
                var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<GameCatalogOptions>>().Value;
                client.BaseAddress = new Uri(options.ApiBaseUrl);
            });
            services.AddSingleton<IGameCatalogProvider, HttpGameCatalogProvider>();
            services.AddHostedService<GameCatalogStartupSync>();

            var jwtKey = configuration["Jwt:Key"];
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudience = configuration["Jwt:Audience"];
            if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32 ||
                string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
                throw new InvalidOperationException("Jwt:Key, Jwt:Issuer, and Jwt:Audience must be configured; Jwt:Key must be at least 32 bytes long.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtIssuer,
                        ValidateAudience = true,
                        ValidAudience = jwtAudience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });
            services.AddAuthorization();

            var quizProvider = configuration["Ai:QuizProvider"] ?? "Http";
            if (string.Equals(quizProvider, "Http", StringComparison.OrdinalIgnoreCase))
            {
                services.AddHttpClient<IAiQuizProvider, HttpAiQuizProvider>(client =>
                {
                    var baseUrl = configuration["Ai:BaseUrl"];
                    if (!string.IsNullOrWhiteSpace(baseUrl)) client.BaseAddress = new Uri(baseUrl);
                });
            }
            else if (string.Equals(quizProvider, "OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                services.AddSingleton<IAiQuizProvider, OpenAiQuizProvider>();
            }
            else
            {
                throw new InvalidOperationException($"Unsupported Ai:QuizProvider '{quizProvider}'. Register its IAiQuizProvider adapter here.");
            }

            // Domain services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IQuizRepository, QuizRepository>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IGotyPredictionRepository, GotyPredictionRepository>();
            services.AddScoped<IQuizAnswerEvaluator, KnowledgeQuizAnswerEvaluator>();
            services.AddScoped<IQuizSessionService, QuizSessionService>();
            services.AddScoped<IQuizScoringPolicy, QuizScoringPolicy>();
            services.AddScoped<IReviewEligibilityPolicy, ReviewEligibilityPolicy>();
            services.AddScoped<IReviewCreationService, ReviewCreationService>();
            services.AddScoped<IGotyPredictionService, GotyPredictionService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            return services;
        }
    }
}
