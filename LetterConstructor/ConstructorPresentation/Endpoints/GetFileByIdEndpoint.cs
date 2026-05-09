using ConstructorUseCases.GetFileById;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ConstructorPresentation.Enpoints;

public static class GetFileByIdEndpoint
{
    public static RouteGroupBuilder MapGetFileById(this RouteGroupBuilder group)
    {
        group.MapGet("/{fileId:guid}", (Guid fileId, IGetFileByIdRequestHandler handler) =>
            {
                try
                {
                    var response = handler.Handle(new GetFileByIdRequest(fileId));
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
            .WithName("GetFileById")
            .WithSummary("Get file by id")
            .WithDescription("Получить файл по идентификатору")
            .WithOpenApi()
            .Produces<GetFileByIdResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
