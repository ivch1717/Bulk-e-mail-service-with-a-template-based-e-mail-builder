using System.ComponentModel.DataAnnotations;
using ConstructorUseCases.ExportBlock;
using ConstructorUseCases.ExportTemplate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ConstructorPresentation.Enpoints;

public static class ExportTemplateEndpoint
{
    public static RouteGroupBuilder MapExportTemplate(this RouteGroupBuilder group)
    {
        group.MapPost("/export", (ExportTemplateRequest request, IExportTemplateRequestHandler handler) =>
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
            .WithName("ExportTemplate")
            .WithSummary("Export Template")
            .WithDescription("Экспортировать шаблона письма")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        return group;
    }
}