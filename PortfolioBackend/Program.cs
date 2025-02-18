using MongoDB.Driver;
using MongoDB.Bson;
using System.Linq;
using System.Collections;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using PortfolioBackend.Models;
using PortfolioBackend.Services;
using Microsoft.AspNetCore;
using AspNetCore.Identity.Mongo;


namespace PortfolioBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.Configure<DatabaseSettings>(
                builder.Configuration.GetSection("MongoDB"));

            builder.Services.AddSingleton<APIServices>();
            builder.Services.AddAuthorization();
          

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            //Identity

            var connectionString = builder.Configuration.GetConnectionString("ConnectionString");
            builder.Services.AddIdentityMongoDbProvider<ApplicationUser, ApplicationRole, string>(identity =>
            {
                identity.Password.RequiredLength = 8;
                identity.Password.RequireUppercase = true;
                identity.Password.RequireDigit = true;

            },
            mongo =>
            {
                mongo.ConnectionString = connectionString;
                // other options
            });


            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            // GET
            app.MapGet("/api/techs", async (APIServices service) =>
            {
                var tech = await service.GetAsync();
                return Results.Ok(tech);
            });
            //get by id
            app.MapGet("/api/tech/{id}", async (APIServices service, string id) =>
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

            });
            // update by id
            app.MapPut("/api/tech{id}", async (APIServices service, TechStack updatedTech, string id) =>
            {
                var storedMessage = await service.GetByIdAsync(id);
                if (storedMessage == null)
                {
                    return Results.NotFound();

                }
                await service.UpdateAsync(id, updatedTech);
                return Results.Ok(storedMessage);
            });
            app.MapPost("/api/newtech", async (APIServices service,TechStack newTech) =>
            {

                await service.CreateAsync(newTech);
                return Results.Ok(newTech);

            });


            app.MapDelete("/api/deletetech{id}", async (APIServices service, string id) =>
            {
                var snus = await service.GetByIdAsync(id);

                if (snus == null)
                {
                    return Results.NotFound();
                }
                await service.RemoveAsync(id);
                return Results.Ok();
            });
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.Run();
        }
    }
}
