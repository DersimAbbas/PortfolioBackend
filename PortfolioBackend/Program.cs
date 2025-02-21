using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PortfolioBackend.Models;
using PortfolioBackend.Services;

namespace PortfolioBackend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var jwtKey = builder.Configuration["Jwt:Key"];
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                throw new InvalidOperationException("JWT configuration is missing in appsettings.json.");
            }

            var key = Encoding.UTF8.GetBytes(jwtKey);
            // Add services to the container.
            builder
                .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                    };
                });

            builder.Services.AddAuthorization();
            builder.Services.Configure<DatabaseSettings>(
                builder.Configuration.GetSection("MongoDB")
            );
            builder.Services.AddSingleton<APIServices>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Identity

            var connectionString = builder
                .Configuration.GetSection("MongoDb:ConnectionString")
                .Value;
            builder
                .Services.AddIdentityMongoDbProvider<ApplicationUser, ApplicationRole, string>(
                    identity =>
                    {
                        identity.Password.RequiredLength = 8;
                        identity.Password.RequireUppercase = true;
                        identity.Password.RequireDigit = true;
                    },
                    mongo =>
                    {
                        mongo.ConnectionString = connectionString;

                        // other options
                    }
                )
                .AddDefaultTokenProviders();
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<
                    UserManager<ApplicationUser>
                >();
                var roleManager = scope.ServiceProvider.GetRequiredService<
                    RoleManager<ApplicationRole>
                >();

                // Check if Admin role exists, if not, create it
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
                }

                // Check if Admin user exists, if not, create it
                var adminUser = await userManager.FindByEmailAsync("admin@portfolio.com");
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            // GET
            app.MapGet(
                "/api/techs",
                async (APIServices service) =>
                {
                    var tech = await service.GetAsync();
                    return Results.Ok(tech);
                }
            );
            //get by id
            app.MapGet(
                "/api/tech/{id}",
                async (APIServices service, string id) =>
                {
                    var snus = await service.GetByIdAsync(id);

                    if (snus == null)
                    {
                        return Results.NotFound(snus);
                    }
                    else
                    {
                        return Results.Ok(snus);
                    }
                }
            );
            // update by id
            app.MapPut(
                "/api/tech{id}",
                async (APIServices service, TechStack updatedTech, string id) =>
                {
                    var storedMessage = await service.GetByIdAsync(id);
                    if (storedMessage == null)
                    {
                        return Results.NotFound();
                    }
                    await service.UpdateAsync(id, updatedTech);
                    return Results.Ok(storedMessage);
                }
            );
            app.MapPost(
                "/api/newtech",
                async (APIServices service, TechStack newTech) =>
                {
                    await service.CreateAsync(newTech);
                    return Results.Ok(newTech);
                }
            );

            app.MapPost(
                "/api/auth/login",
                async (
                    SignInManager<ApplicationUser> signInManager,
                    UserManager<ApplicationUser> userManager,
                    IConfiguration config,
                    LoginDto loginDto
                ) =>
                {
                    var user = await userManager.FindByNameAsync(loginDto.Username);
                    if (user == null)
                    {
                        return Results.Problem(
                            "Invalid Credentials.",
                            statusCode: StatusCodes.Status401Unauthorized
                        );
                    }

                    var result = await signInManager.PasswordSignInAsync(
                        user,
                        loginDto.Password,
                        true,
                        false
                    );
                    if (!result.Succeeded)
                    {
                        return Results.Problem(
                            "Invalid Credentials.",
                            statusCode: StatusCodes.Status401Unauthorized
                        );
                    }

                    // Create JWT Token
                    var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]);
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(
                            ClaimTypes.Role,
                            "Admin"
                        ) // Assign roles dynamically if needed
                        ,
                    };

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(claims),
                        Expires = DateTime.UtcNow.AddHours(1), // Token expiry
                        SigningCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(key),
                            SecurityAlgorithms.HmacSha256Signature
                        ),
                        Issuer = config["Jwt:Issuer"],
                        Audience = config["Jwt:Audience"],
                    };

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var jwtToken = tokenHandler.WriteToken(token);

                    return Results.Ok(new { Token = jwtToken });
                }
            );

            app.MapDelete(
                "/api/deletetech{id}",
                async (APIServices service, string id) =>
                {
                    var snus = await service.GetByIdAsync(id);

                    if (snus == null)
                    {
                        return Results.NotFound();
                    }
                    await service.RemoveAsync(id);
                    return Results.Ok();
                }
            );

            app.UseSwagger();
            app.UseSwaggerUI();

            app.Run();
        }
    }
}
