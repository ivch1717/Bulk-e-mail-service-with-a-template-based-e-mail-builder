using ConstructorUseCases.AddFile;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ConstructorPresentation.Enpoints;

public static class AddFileEndpoint
{
    public static RouteGroupBuilder MapAddFile(this RouteGroupBuilder group)
    {
        group.MapPost("", (AddFileRequest request, IAddFileRequestHandler handler) =>
            {
                try
                {
                    var response = handler.Handle(request);
                    return Results.Created($"/files/{response.FileId}", response);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (FileNameAlreadyExistsException ex)
                {
                    return Results.Conflict(new { error = ex.Message });
                }
            })
            .WithName("AddFile")
            .WithSummary("Add file")
            .WithDescription("Сохранить html-файл (блок или шаблон) в БД")
            .WithOpenApi()
            .Produces<AddFileResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

        return group;
    }
}
