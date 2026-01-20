using Microsoft.Extensions.DependencyInjection;

namespace UseCases;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IAddressParser, AddressParser>();
        services.AddScoped<IUploadDataRequestHandler, UploadDataRequestHandler>();

        return services;
    }
}