using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Read JWT settings from appsettings.json
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"]!;
var audience = jwtSection["Audience"]!;
var secret = jwtSection["Secret"]!;

// Build token validation parameters (like JwtBearer would)
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = issuer,
    ValidateAudience = true,
    ValidAudience = audience,
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = key,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero
};

// Swagger
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// YARP - load routes/clusters from config
services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

//
// Simple auth middleware JUST for routes that need auth
//
app.Use(async (context, next) =>
{
    // For now, we only protect /whoami; later we’ll protect proxied routes
    if (context.Request.Path.StartsWithSegments("/whoami"))
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) ||
            !authHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing or invalid Authorization header");
            return;
        }

        var token = authHeader.ToString()["Bearer ".Length..].Trim();
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var principal = handler.ValidateToken(token, tokenValidationParameters, out _);
            context.User = principal;
        }
        catch (Exception)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid token");
            return;
        }
    }

    await next();
});

// Health (no auth)
app.MapGet("/health", () => Results.Ok("ok"));

// WhoAmI (requires valid JWT via the middleware above)
app.MapGet("/whoami", (HttpContext ctx) =>
{
    var user = ctx.User;
    if (user?.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    var name = user.Identity?.Name ?? user.FindFirst("name")?.Value ?? "unknown";
    var tenant = user.FindFirst("tenant")?.Value ?? "n/a";
    var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

    return Results.Ok(new
    {
        name,
        tenant,
        roles
    });
});

app.MapReverseProxy();
app.Run();
