using Microsoft.Extensions.DependencyInjection;
using NPOI.POIFS.Crypt.Dsig;

namespace UseCases;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IAddressParser, AddressParser>();
        services.AddScoped<ITemplateParser, TemplateParser>();
        services.AddScoped<IDataParser, DataParser>();
        // services.AddScoped<IUploadDataRequestHandler, UploadDataRequestHandler>();
        services.AddScoped<IUploadTemplateRequestHandler, UploadTemplateRequestHandler>();
        services.AddScoped<ITableExtracter, TableExtracter>();
        services.AddScoped<IProcessEmailCreationRequestHandler, ProcessEmailCreationRequestHandler>();
        return services;
    }
}