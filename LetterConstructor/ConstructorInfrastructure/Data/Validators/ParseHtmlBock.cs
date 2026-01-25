using AngleSharp.Html.Parser;
using ConstructorUseCases.ExportBlock;

namespace ConstructureInfrastructure.Data.Validators;

public class ParseHtmlBock : IParseHtmlBock
{
    private readonly HtmlParser _parser = new();

    public string? Parse(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        try
        {
            var contextDoc = _parser.ParseDocument("<div></div>");
            var context = contextDoc.QuerySelector("div")!;

            var nodes = _parser.ParseFragment(html, context);

            foreach (var node in nodes.ToArray())
                context.AppendChild(node);

            return context.InnerHtml;

        }
        catch (HtmlParseException)
        {
            return null;
        }
    }
}