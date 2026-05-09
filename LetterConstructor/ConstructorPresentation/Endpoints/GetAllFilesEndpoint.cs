using ConstructorUseCases.GetAllFiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ConstructorPresentation.Enpoints;

public static class GetAllFilesEndpoint
{
    public static RouteGroupBuilder MapGetAllFiles(this RouteGroupBuilder group)
    {
        group.MapGet("", (IGetAllFilesRequestHandler handler) =>
            {
                try
                {
                    var response = handler.Handle(new GetAllFilesRequest());
                    return Results.Ok(response);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("GetAllFiles")
            .WithSummary("Get all files")
            .WithDescription("Получить список всех файлов (имя, дата создания), отсортированных по дате создания убыванию")
            .WithOpenApi()
            .Produces<GetAllFilesResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return group;
    }
}
