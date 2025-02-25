using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PortfolioBackend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes, IConfiguration config)
    {
        routes.MapPost("/api/auth/login", async (
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            LoginDto loginDto) =>
        {
            var user = await userManager.FindByNameAsync(loginDto.Username);
            if (user == null)
            {
                return Results.Problem("Invalid Credentials.", statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await signInManager.PasswordSignInAsync(user, loginDto.Password, true, false);
            if (!result.Succeeded)
            {
                return Results.Problem("Invalid Credentials.", statusCode: StatusCodes.Status401Unauthorized);
            }

            // Create JWT Token
            var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"],
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return Results.Ok(new { Token = jwtToken });
        });

        return routes;
    }
}
