using Microsoft.AspNetCore.Http;

namespace UseCases.TemplateUtilities;

public class TemplateFactory : ITemplateFactory
{
    public ITemplate Create(IFormFile file)
    {
        return new HtmlTemplate(file);
    }
}