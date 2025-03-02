using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PortfolioBackend.Models;
using PortfolioBackend.Services;

public static class TechStackEndpoints
{
    public static IEndpointRouteBuilder MapTechStackEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet(
            "/api/techs",
            async (APIServices service) =>
            {
                var tech = await service.GetAsync();
                return Results.Ok(tech);
            }
        );

        routes.MapGet(
            "/api/projects",
            async (APIServices service) =>
            {
                var project = await service.GetProjectsAsync();
                return Results.Ok(project);
            }
        );

        routes.MapGet(
            "/api/tech/{id}",
            async (APIServices service, string id) =>
            {
                var tech = await service.GetByIdAsync(id);
                return tech is null ? Results.NotFound() : Results.Ok(tech);
            }
        );

        routes.MapPut(
            "/api/updatetech{id}",
            async (APIServices service, TechStack updatedTech, string id) =>
            {
                var storedTech = await service.GetByIdAsync(id);
                if (storedTech == null)
                {
                    return Results.NotFound();
                }
                await service.UpdateAsync(id, updatedTech);
                return Results.Ok(storedTech);
            }
        );

        routes.MapPost(
            "/api/newtech",
            async (APIServices service, TechStack newTech) =>
            {
                await service.CreateAsync(newTech);
                return Results.Ok(newTech);
            }
        );

        routes.MapDelete(
            "/api/deletetech{id}",
            async (APIServices service, string id) =>
            {
                var tech = await service.GetByIdAsync(id);
                if (tech == null)
                {
                    return Results.NotFound();
                }
                await service.RemoveAsync(id);
                return Results.Ok();
            }
        );

        return routes;
    }
}
