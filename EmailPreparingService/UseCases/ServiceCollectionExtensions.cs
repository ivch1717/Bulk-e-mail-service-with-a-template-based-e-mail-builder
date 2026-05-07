using Microsoft.Extensions.DependencyInjection;
using UseCases.CreateSmtpProfile;
using UseCases.DeleteSmtpProfile;
using UseCases.GetAllCampaigns;
using UseCases.GetAllSmtpProfiles;
using UseCases.GetCampaign;
using UseCases.GetPreview;
using UseCases.Preparation.ExtractTableHeaders;
using UseCases.Preparation.UploadTemplate;
using UseCases.TemplateUtilities;
using UploadTemplateRequestHandler = UseCases.Preparation.UploadTemplate.UploadTemplateRequestHandler;

namespace UseCases;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        // Factories.
        services.AddScoped<ITableFactory, TableFactory>();
        services.AddScoped<ITemplateFactory, TemplateFactory>();
        
        // EmailPreparationEndpoints.
        services.AddScoped<IUploadTemplateRequestHandler, UploadTemplateRequestHandler>();
        services.AddScoped<IExtractTableHeadersRequestHandler,  ExtractTableHeadersRequestHandler>();
        services.AddScoped<IGetPreviewRequestHandler,   GetPreviewRequestHandler>();
        services.AddScoped<ISendRequestHandler, SendRequestHandler>();
        
        // StatisticsEndpoints.
        services.AddScoped<ITrackOpenRequestHandler, TrackOpenRequestHandler>();
        services.AddScoped<IGetAllCampaignsRequestHandler, GetAllCampaignsRequestHandler>();
        services.AddScoped<IGetCampaignRequestHandler, GetCampaignRequestHandler>();
        
        // SmtpProfileEndpoints.
        services.AddScoped<ICreateSmtpProfileRequestHandler, CreateSmtpProfileRequestHandler>();
        services.AddScoped<IDeleteSmtpProfileRequestHandler, DeleteSmtpProfileRequestHandler>();
        services.AddScoped<IGetAllSmtpProfilesRequestHandler, GetAllSmtpProfilesRequestHandler>();
        
        return services;
    }
}