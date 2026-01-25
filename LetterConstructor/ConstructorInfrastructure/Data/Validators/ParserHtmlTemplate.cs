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
            var doc = _parser.ParseDocument(html);
            return doc.DocumentElement.OuterHtml;
        }
        catch (HtmlParseException)
        {
            return null;
        }
    }
}