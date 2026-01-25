using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ConstructorUseCases.ExportTemplate;

public class ExportTemplateRequestHandler : IExportTemplateRequestHandler
{
    public IParserHtmlTemplate _parserHtmlTemplate;

    public ExportTemplateRequestHandler(IParserHtmlTemplate parserHtmlTemplate)
    {
        _parserHtmlTemplate = parserHtmlTemplate;
    }
    
    public ExportTemplateResponse Handle(ExportTemplateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Html))
        {
            throw new ValidationException("Invalid html");
        }
        string? html = _parserHtmlTemplate.Parse(request.Html);
        if (html is null)
        {
            throw new ValidationException("Invalid html");
        }
        
        
        var bodyInnerHtml = ExtractBodyInnerHtm(html);
        if (bodyInnerHtml is null)
        {
            throw new ValidationException("Invalid html");
        }
        
        var bytes = Encoding.UTF8.GetBytes(html);
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