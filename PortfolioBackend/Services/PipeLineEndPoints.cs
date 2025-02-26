using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using PortfolioBackend.Models;
using PortfolioBackend.Services;

public static class PipelineEndpoints
{
    public static IEndpointRouteBuilder MapPipelineEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/pipelinestages", async (APIServices service) =>
        {
            var response = await service.GetAllStagesAsync();
            return Results.Ok(response) ?? Results.NotFound(response);

        });

        routes.MapPost("/pipelinestages", async (APIServices service, PipeLineStage stage) =>
        {
            var response = await service.CreateStageAsync(stage);
            return Results.Created($"/pipelinestages/{stage.Id}", stage) ?? Results.BadRequest(response);
        });

        // NEW: Bulk insert endpoint
        routes.MapPost("/pipelinestages/bulk", async (APIServices service, List<PipeLineStage> stages) =>
        {
            await service.CreateManyStagesAsync(stages);
            return Results.Created("/pipelinestages", stages);
        });
        routes.MapPut("/updatepipeline/{id}", async (APIServices service, string id, PipeLineStage pipeline) =>
        {

            await service.UpdatePipelineAsync(id, pipeline);
        });

        return routes;
    }
}
