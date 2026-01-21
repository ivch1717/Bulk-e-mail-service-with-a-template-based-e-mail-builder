using System.ComponentModel.DataAnnotations;
using System.Text;
using ConstructorUseCases.Common;

namespace ConstructorUseCases.ExportTemplate;

public class ExportTemplateRequestHandler : IExportTemplateRequestHandler
{
    public IHtmlValidatorBody _htmlValidatorBody;
    public IHtmlValidatorTemplate _htmlValidatorTemplate;

    public ExportTemplateRequestHandler(IHtmlValidatorBody htmlValidatorBody,  IHtmlValidatorTemplate htmlValidatorTemplate)
    {
        _htmlValidatorBody = htmlValidatorBody;
        _htmlValidatorTemplate = htmlValidatorTemplate;
    }
    
    public ExportTemplateResponse Handle(ExportTemplateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Html))
        {
            throw new ValidationException("Invalid html");
        }
        
        if (!_htmlValidatorTemplate.IsValid(request.Html))
        {
            throw new ValidationException("Invalid html");
        }

        var bodyInnerHtml = ExtractBodyInnerHtm(request.Html);
        if (bodyInnerHtml is null)
        {
            throw new ValidationException("Invalid html");
        }

        if (!_htmlValidatorBody.IsValid(bodyInnerHtml))
        {
            throw new ValidationException("Invalid html");
        }
        
        var bytes = Encoding.UTF8.GetBytes(request.Html);
        var stream = new MemoryStream(bytes);

        return new ExportTemplateResponse(
            Content: stream,
            FileName: "template.html",
            ContentType: "text/html; charset=utf-8"
        );
    }

    private static string ExtractBodyInnerHtm(string html)
    {
        var openBodyCount = CountOccurrences(html, "<body");
        var closeBodyCount = CountOccurrences(html, "</body>");

        if (openBodyCount != 1 || closeBodyCount != 1)
        {
            throw new ValidationException("HTML must contain exactly one <body>");
        }

        var openStart = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
        var openEnd = html.IndexOf('>', openStart);
        if (openEnd < 0)
        {
            throw new ValidationException("Invalid <body> tag");
        }

        var closeStart = html.IndexOf("</body>", openEnd + 1, StringComparison.OrdinalIgnoreCase);

        return html.Substring(openEnd + 1, closeStart - (openEnd + 1));
    }

    private static int CountOccurrences(string text, string value)
    {
        int count = 0;
        int index = 0;

        while ((index = text.IndexOf(value, index, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

}