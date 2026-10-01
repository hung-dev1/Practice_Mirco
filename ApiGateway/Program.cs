using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace ApiGateway
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load ocelot.json
            builder.Configuration
                   .SetBasePath(builder.Environment.ContentRootPath)
                   .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
                   .AddEnvironmentVariables();

            // JWT - dung de Ocelot xac thuc token tren cac route bao ve
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var secretKey  = jwtSection["SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");

            builder.Services
                   .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                   .AddJwtBearer("Bearer", options =>
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

            // Ocelot (khong can Consul provider - dung DownstreamHostAndPorts truc tiep)
            builder.Services.AddOcelot(builder.Configuration);

            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            // Ocelot phai la middleware cuoi cung
            await app.UseOcelot();

            app.Run();
        }
    }
}