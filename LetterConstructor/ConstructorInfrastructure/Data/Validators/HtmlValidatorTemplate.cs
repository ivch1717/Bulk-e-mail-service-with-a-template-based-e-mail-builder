using AngleSharp.Html.Parser;
using ConstructorUseCases.Common;

namespace ConstructureInfrastructure.Data.Validators;

public class HtmlValidatorBody : IHtmlValidatorBody
{
    private readonly HtmlParser _parser = new();

    public bool IsValid(string html)
    {
        try
        {
            var contextDoc = _parser.ParseDocument("<div></div>");
            var context = contextDoc.QuerySelector("div")!;
            _parser.ParseFragment(html, context);

            return true;
        }
        catch
        {
            return false;
        }
    }
}