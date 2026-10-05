using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Service.Implement
{   
    public class AuthServiceImpl : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthServiceImpl(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ─── Sign Up ───────────────────────────────────────────────────────────
        public async Task<SignUpResponse> SignUpAsync(SignUpRequest request)
        {
            // Check email duplicate
            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email);
            if (emailExists)
                throw new InvalidOperationException("Email already exists.");

            // Default role: Student (id = 2)
            const int defaultRoleId = 2;
            Role? role = await _context.Roles.FindAsync(defaultRoleId);
            if (role is null)
                throw new InvalidOperationException("Default role 'Student' not found in database.");

            // Hash password with BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Password = hashedPassword,
                Email    = request.Email,
                RoleId   = role.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new SignUpResponse
            {
                Username = user.Username,
                Message  = "User registered successfully."
            };
        }

        // ─── Sign In ───────────────────────────────────────────────────────────
        public async Task<SignInResponse> SignInAsync(SignInRequest request)
        {
            // Find user (eager-load role)
            User? user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid username or password.");

            string accessToken  = GenerateAccessToken(user);
            string refreshToken = GenerateRefreshToken();

            return new SignInResponse
            {
                AccessToken  = accessToken,
                RefreshToken = refreshToken
            };
        }

        // ─── Access Token ──────────────────────────────────────────────────────
        private string GenerateAccessToken(User user)
        {
            string secretKey  = GetSecretKey();
            string issuer     = _configuration["Jwt:Issuer"]   ?? "AuthService";
            string audience   = _configuration["Jwt:Audience"] ?? "AuthService";
            int expiryMinutes = int.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"] ?? "60");

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,        user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email,      user.Email),
                new Claim(ClaimTypes.Role,                    user.Role.Name),
                new Claim(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer:             issuer,
                audience:           audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ─── Refresh Token ─────────────────────────────────────────────────────
        // Cung cau truc JWT voi AccessToken nhung khong chua thong tin user,
        // chi chua jti (unique id) va thoi gian het han.
        private string GenerateRefreshToken()
        {
            string secretKey = GetSecretKey();
            string issuer    = _configuration["Jwt:Issuer"]   ?? "AuthService";
            string audience  = _configuration["Jwt:Audience"] ?? "AuthService";
            int expiryDays   = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("token_type", "refresh")
            };

            var token = new JwtSecurityToken(
                issuer:             issuer,
                audience:           audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddDays(expiryDays),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GetSecretKey() =>
            _configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
    }
}
