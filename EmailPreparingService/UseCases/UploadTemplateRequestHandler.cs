using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace UseCases;

public class UploadTemplateRequestHandler : IUploadTemplateRequestHandler
{
    public List<string> Handle(UploadTemplateRequest request)
    {
        var list = Regex.Matches(ReadText(request.template), @"\[\[(.*?)\]\]");
        List<string> result = [];
        foreach (var match in list)
        {
            result.Add(match.ToString().Substring(2, match.ToString().Length - 4));
        }

        return result;
    }
    
    public string ReadText(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        return reader.ReadToEnd();
    }
}