using System.Text;
using AuthService.Data;
using AuthService.Entities;
using AuthService.Extensions;
using AuthService.Service.Interface;
using AuthService.Service.Implement;
using AuthService.Repository.Interface;
using AuthService.Repository.Implement;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthService
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
            var jwtSection = builder.Configuration.GetSection("Jwt");
            string secretKey = jwtSection["SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                                                  Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // ── Application Services 
            builder.Services.AddScoped<IAuthService, AuthServiceImpl>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserServiceImpl>();

            // ── Consul ────────────────────────────────────────────────────────
            builder.Services.AddConsulClient(builder.Configuration);

            // ── Controllers & OpenAPI ─────────────────────────────────────────
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            // ── Health check
            builder.Services.AddHealthChecks();

            var app = builder.Build();

            // ── Auto Create Schema + Seed Roles
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();

                // Seed roles nếu chưa có
                if (!db.Roles.Any())
                {
                    db.Roles.AddRange(
                        new Role { Name = "ADMIN" },
                        new Role { Name = "STUDENT" },
                        new Role { Name = "TEACHER" }
                    );
                    db.SaveChanges();
                }
            }

            // ── HTTP Pipeline ─────────────────────────────────────────────────
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            // ── Health check endpoint (dùng cho Consul) ───────────────────────
            // Phải đặt trước UseHttpsRedirection để Consul gọi HTTP không bị redirect
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

