using Microsoft.AspNetCore.Http;

namespace UseCases.TemplateUtilities;

public class TemplateFactory : ITemplateFactory
{
    public ITemplate Create(IFormFile file, bool tracking)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return extension switch
        {
            ".html" => tracking ? new HtmlTrackingTemplate(file) : new HtmlTemplate(file),
            _ => throw new ArgumentException($"Неподдерживаемый формат файла: {extension}.")
        };
    }
}