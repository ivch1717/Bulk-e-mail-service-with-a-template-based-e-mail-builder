using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using UseCases.TemplateUtilities;

namespace UseCases.UploadTemplate;

public partial class UploadTemplateRequestHandler(ITemplateFactory templateFactory) : IUploadTemplateRequestHandler
{
    public List<string> Handle(UploadTemplateRequest request)
    {
        var template = templateFactory.Create(request.template, false);
        var list = GetVariables().Matches(ReadText(request.template));
        HashSet<string> result =
        [
            "email"
        ];
        foreach (var match in list)
        {
            result.Add(match!.ToString()!.Substring(2, match.ToString()!.Length - 4));
        }
        
        return result.ToList();
    }

    private static string ReadText(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        return reader.ReadToEnd();
    }

    [GeneratedRegex(@"\[\[(.*?)\]\]")]
    private static partial Regex GetVariables();
}