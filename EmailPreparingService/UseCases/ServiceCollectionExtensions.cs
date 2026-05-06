using Microsoft.Extensions.DependencyInjection;
using NPOI.POIFS.Crypt.Dsig;
using UseCases.CreateSmtpProfile;
using UseCases.DeleteSmtpProfile;
using UseCases.GetAllCampaigns;
using UseCases.GetAllSmtpProfiles;
using UseCases.GetCampaign;
using UseCases.GetPreview;
using UseCases.Preparation.ExtractTableHeaders;
using UseCases.Preparation.UploadTemplate;
using UseCases.TemplateUtilities;
using UseCases.UploadTemplate;
using UploadTemplateRequestHandler = UseCases.Preparation.UploadTemplate.UploadTemplateRequestHandler;

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
        services.AddScoped<ICreateSmtpProfileRequestHandler, CreateSmtpProfileRequestHandler>();
        services.AddScoped<IDeleteSmtpProfileRequestHandler, DeleteSmtpProfileRequestHandler>();
        services.AddScoped<IGetAllSmtpProfilesRequestHandler, GetAllSmtpProfilesRequestHandler>();
        return services;
    }
}