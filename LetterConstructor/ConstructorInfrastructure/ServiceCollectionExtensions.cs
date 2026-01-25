using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConstructorUseCases.ExportTemplate;
using ConstructorUseCases.ExportBlock;
using ConstructureInfrastructure.Data.Validators;
namespace ConstructureInfrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConstructorInfrastructure(
        this IServiceCollection services
        )
    {
        services.AddScoped<IParseHtmlBock, ParseHtmlBock>();
        services.AddScoped<IParserHtmlTemplate, ParserHtmlTemplate>();

        return services;
    }
}