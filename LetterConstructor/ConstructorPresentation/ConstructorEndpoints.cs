using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ConstructorPresentation.Enpoints;

namespace ConstructorPresentation;

public static class ConstructorEndpoints
{
    public static WebApplication MapConstructorEndpoints(this WebApplication app)
    {
        app.MapGroup("/blocks")
            .WithTags("Blocks")
            .MapImportBlock()
            .MapExportBlock();

        app.MapGroup("/templates")
            .WithTags("Templates")
            .MapExportTemplate();

        app.MapGroup("/files")
            .WithTags("Files")
            .MapAddFile()
            .MapGetAllFiles()
            .MapGetFileById()
            .MapDeleteFileById();

        return app;
    }
}
