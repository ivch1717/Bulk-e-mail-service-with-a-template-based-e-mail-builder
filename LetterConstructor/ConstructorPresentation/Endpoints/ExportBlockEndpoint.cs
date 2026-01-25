using System.ComponentModel.DataAnnotations;
using ConstructorUseCases.ExportBlock;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ConstructorPresentation.Enpoints;

public static class ExportBlockEndpoint
{
    public static RouteGroupBuilder MapExportBlock(this RouteGroupBuilder group)
    {
        group.MapPost("/export", (ExportBlockRequest request, IExportBlockRequestHandler handler) =>
        {
            try
            {
                var response = handler.Handle(request);
                return Results.File(response.Content, response.ContentType, response.FileName);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("ExportBlock")
        .WithSummary("Export block")
        .WithDescription("Экспортировать html блок")
        .WithOpenApi()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
        return group;
    }
}