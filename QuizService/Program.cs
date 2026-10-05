using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuizService.Data;
using QuizService.Extensions;
using QuizService.Repository.Implement;
using QuizService.Repository.Interface;
using QuizService.Service.Implement;
using QuizService.Service.Interface;
using QuizService.ServiceClients;
using Refit;

namespace QuizService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Database ──────────────────────────────────────────────────────
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IQuizRepository, QuizRepository>();
            builder.Services.AddScoped<IQuizService, QuizServiceImpl>();

            // ── QuestionService client ────────────────────────────────────────
            var questionServiceUrl = builder.Configuration["Services:QuestionService"]
                ?? throw new InvalidOperationException("Services:QuestionService is not configured.");

            builder.Services.AddRefitGeneratedClient<IQuestionClient>()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri(questionServiceUrl));

            // ── JWT Authentication ────────────────────────────────────────────
            var jwtSection = builder.Configuration.GetSection("Jwt");
            string secretKey = jwtSection["SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = jwtSection["Issuer"],
                    ValidAudience            = jwtSection["Audience"],
                    IssuerSigningKey         = new SymmetricSecurityKey(
                                                   Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew                = TimeSpan.Zero
                };
            });

            // ── Consul ────────────────────────────────────────────────────────
            builder.Services.AddConsulClient(builder.Configuration);

            // ── Controllers & OpenAPI ─────────────────────────────────────────
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            // ── Health check ──────────────────────────────────────────────────
            builder.Services.AddHealthChecks();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            // ── Health check endpoint (dùng cho Consul) ───────────────────────
            app.MapHealthChecks("/health");

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // ── Đăng ký lên Consul ────────────────────────────────────────────
            app.UseConsulRegistration(builder.Configuration, app.Lifetime);

            app.Run();
        }
    }
}
