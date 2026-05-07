using Microsoft.Extensions.DependencyInjection;
using UseCases.Preparation.ExtractTableHeaders;
using UseCases.Preparation.GetPreview;
using UseCases.Preparation.Send;
using UseCases.Preparation.UploadTemplate;
using UseCases.SmtpProfile.CreateSmtpProfile;
using UseCases.SmtpProfile.DeleteSmtpProfile;
using UseCases.SmtpProfile.GetAllSmtpProfiles;
using UseCases.Statistics.GetAllCampaigns;
using UseCases.Statistics.GetCampaign;
using UseCases.Statistics.TrackOpen;
using UseCases.TableUtilities;
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