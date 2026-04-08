using Microsoft.AspNetCore.Http;

namespace UseCases.TemplateUtilities;

public interface ITemplateFactory
{
    public ITemplate Create(IFormFile file);
}