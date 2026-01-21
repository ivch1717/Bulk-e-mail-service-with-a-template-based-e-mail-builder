using System.ComponentModel.DataAnnotations;
using ConstructorUseCases.ExportBlock;
using ConstructorUseCases.ImportBlock;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ConstructorPresentation.Enpoints;

public static class ImportBlockEndpoint
{
    public static RouteGroupBuilder MapImportBlock(this RouteGroupBuilder group)
    {
        group.MapPost("/import", (ImportBlockRequest request, IImportBlockRequestHandler handler) =>
            {
                try
                {
                    var response = handler.Handle(request);
                    return Results.Ok(response);
                }
                catch (ValidationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("ImportBlock")
            .WithSummary("Import block")
            .WithDescription("Импортировать html блок")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        return group;
    }
}