using ConstructorUseCases.AddFile;
using ConstructorUseCases.DeleteFileById;
using ConstructorUseCases.ExportBlock;
using ConstructorUseCases.ExportTemplate;
using ConstructorUseCases.GetAllFiles;
using ConstructorUseCases.GetFileById;
using ConstructureInfrastructure.Data.Db;
using ConstructureInfrastructure.Data.Repositories;
using ConstructureInfrastructure.Data.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConstructureInfrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConstructorInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        services.AddDbContext<ConstructorDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IParseHtmlBock, ParseHtmlBock>();
        services.AddScoped<IParserHtmlTemplate, ParserHtmlTemplate>();

        services.AddScoped<IAddFileRepository, AddFileRepository>();
        services.AddScoped<IDeleteFileByIdRepository, DeleteFileByIdRepository>();
        services.AddScoped<IGetFileByIdRepository, GetFileByIdRepository>();
        services.AddScoped<IGetAllFilesRepository, GetAllFilesRepository>();

        services.AddHostedService<MigrationRunner>();

        return services;
    }
}
