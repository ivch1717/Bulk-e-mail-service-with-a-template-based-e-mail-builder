using Microsoft.AspNetCore.Http;

namespace UseCases.TemplateUtilities;

public class TemplateFactory : ITemplateFactory
{
    public ITemplate Create(IFormFile file, bool tracking)
    {
        return tracking ? new HtmlTrackingTemplate(file) : new HtmlTemplate(file);
    }
}