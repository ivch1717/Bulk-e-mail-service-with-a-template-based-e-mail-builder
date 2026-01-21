using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConstructorUseCases.Common;
using ConstructorUseCases.ExportTemplate;
using ConstructureInfrastructure.Data.Validators;
namespace ConstructureInfrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConstructorInfrastructure(
        this IServiceCollection services
        )
    {
        services.AddScoped<IHtmlValidatorBody, HtmlValidatorBody>();
        services.AddScoped<IHtmlValidatorTemplate, HtmlValidatorTemplate>();

        return services;
    }
}