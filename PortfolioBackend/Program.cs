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

            // JWT settings
            var jwtKey = builder.Configuration["Jwt:Key"];
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                throw new InvalidOperationException("JWT configuration is missing in appsettings.json.");
            }

            var key = Encoding.UTF8.GetBytes(jwtKey);
            // Add services to the container.
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("MongoDB"));
            builder.Services.AddSingleton<APIServices>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Identity configuration using MongoDB
            var connectionString = builder.Configuration.GetSection("MongoDb:ConnectionString").Value;
            builder.Services.AddIdentityMongoDbProvider<ApplicationUser, ApplicationRole, string>(
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
            ).AddDefaultTokenProviders();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

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

            // --- Endpoint mappings moved to extension methods ---
            
            // TechStack endpoints
            app.MapTechStackEndpoints();

            // Authentication endpoints
            app.MapAuthEndpoints(app.Configuration);

           
            app.MapPipelineEndpoints();
           

            app.Run();
        }
    }
}
