using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using UseCases.TemplateUtilities;

namespace UseCases;

public class UploadTemplateRequestHandler : IUploadTemplateRequestHandler
{
    private readonly ITemplateFactory _templateFactory;
    
    public UploadTemplateRequestHandler(ITemplateFactory templatefactory)
    {
        _templateFactory = templatefactory;
    }
    
    public List<string> Handle(UploadTemplateRequest request)
    {
        var template = _templateFactory.Create(request.template, false);
        var list = Regex.Matches(ReadText(request.template), @"\[\[(.*?)\]\]");
        HashSet<string> result = [];
        result.Add("email");
        foreach (var match in list)
        {
            result.Add(match.ToString().Substring(2, match.ToString().Length - 4));
        }

        return result.ToList();
    }
    
    public string ReadText(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        return reader.ReadToEnd();
    }
}