using AngleSharp.Html.Parser;
using ConstructorUseCases.ExportTemplate;

namespace ConstructureInfrastructure.Data.Validators;

public class HtmlValidatorTemplate : IHtmlValidatorTemplate
{
    private readonly HtmlParser _parser = new();

    public bool IsValid(string html)
    {
        try
        {
            var doc = _parser.ParseDocument(html);
            return true;
        }
        catch
        {
            return false;
        }
    }
}