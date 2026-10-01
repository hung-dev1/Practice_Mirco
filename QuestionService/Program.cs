using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestionService.Data;
using QuestionService.Extensions;
using QuestionService.Repository.Interface;
using QuestionService.Repository.Implement;
using QuestionService.Service.Interface;
using QuestionService.Service.Implement;

namespace QuestionService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Database ──────────────────────────────────────────────────────
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ── JWT Authentication ────────────────────────────────────────────
            // QuestionService chỉ xác thực token do AuthService phát — không tự phát token
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

            // ── Repository & Service ──────────────────────────────────────────
            builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.Services.AddScoped<IQuestionService, QuestionServiceImpl>();

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
