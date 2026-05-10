using AngleSharp.Html;
using AngleSharp.Html.Parser;
using ConstructorUseCases.ExportTemplate;

namespace ConstructureInfrastructure.Data.Validators;

public class ParserHtmlTemplate : IParserHtmlTemplate
{
    private readonly HtmlParser _parser = new(); 
    
    public string? Parse(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        try
        {
            var hasHtmlTag = html.IndexOf("<html", StringComparison.OrdinalIgnoreCase) >= 0;
            var hasBodyTag = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase) >= 0;

            var input = hasHtmlTag || hasBodyTag
                ? html
                : $"<!DOCTYPE html><html><head></head><body>{html}</body></html>";

            var doc = _parser.ParseDocument(input);
            using var writer = new StringWriter();
            doc.ToHtml(writer, HtmlMarkupFormatter.Instance);
            return writer.ToString();
        }
        catch (HtmlParseException)
        {
            return null;
        }
    }
}