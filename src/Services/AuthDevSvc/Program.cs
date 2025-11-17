using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthDevSvc;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Read JWT settings
        var jwtSection = builder.Configuration.GetSection("Jwt");
        var issuer   = jwtSection["Issuer"]!;
        var audience = jwtSection["Audience"]!;
        var secret   = jwtSection["Secret"]!; // dev-only

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        // Simple health endpoint
        app.MapGet("/health", () => "ok");

        // POST /auth/token -> issue JWT
        app.MapPost("/auth/token", (LoginRequest request) =>
        {
            var now = DateTimeOffset.UtcNow;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, request.Username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("name", request.Username),
                new("tenant", "dev-tenant-1")
            };

            if (request.Roles is { Length: > 0 })
            {
                claims.AddRange(request.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
            }

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: now.AddHours(1).UtcDateTime,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);

            return Results.Ok(new
            {
                access_token = tokenString,
                token_type   = "Bearer",
                expires_in   = 3600,
                issued_at    = now.ToString("O")
            });
        });

        app.Run();
    }

    public record LoginRequest(string Username, string Password, string[]? Roles);
}
