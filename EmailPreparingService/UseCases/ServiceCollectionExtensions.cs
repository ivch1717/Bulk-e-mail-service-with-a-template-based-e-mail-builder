using Microsoft.Extensions.DependencyInjection;
using NPOI.POIFS.Crypt.Dsig;
using UseCases.ExtractTableHeaders;
using UseCases.GetAllCampaigns;
using UseCases.GetCampaign;
using UseCases.GetPreview;
using UseCases.TemplateUtilities;
using UseCases.UploadTemplate;

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
        services.AddScoped<ITableFactory, TableFactory>();
        services.AddScoped<IExtractTableHeadersRequestHandler,  ExtractTableHeadersRequestHandler>();
        services.AddScoped<IGetPreviewRequestHandler,   GetPreviewRequestHandler>();
        services.AddScoped<ITemplateFactory, TemplateFactory>();
        services.AddScoped<ISendRequestHandler, SendRequestHandler>();
        services.AddScoped<ITrackOpenRequestHandler, TrackOpenRequestHandler>();
        services.AddScoped<IGetAllCampaignsRequestHandler, GetAllCampaignsRequestHandler>();
        services.AddScoped<IGetCampaignRequestHandler, GetCampaignRequestHandler>();
        return services;
    }
}