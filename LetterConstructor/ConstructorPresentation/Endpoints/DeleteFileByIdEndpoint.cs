using ConstructorUseCases.DeleteFileById;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ConstructorPresentation.Enpoints;

public static class DeleteFileByIdEndpoint
{
    public static RouteGroupBuilder MapDeleteFileById(this RouteGroupBuilder group)
    {
        group.MapDelete("/{fileId:guid}", (Guid fileId, IDeleteFileByIdRequestHandler handler) =>
            {
                try
                {
                    var response = handler.Handle(new DeleteFileByIdRequest(fileId));
                    return Results.Ok(response);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = ex.Message });
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("DeleteFileById")
            .WithSummary("Delete file by id")
            .WithDescription("Удалить файл по идентификатору")
            .WithOpenApi()
            .Produces<DeleteFileByIdResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
