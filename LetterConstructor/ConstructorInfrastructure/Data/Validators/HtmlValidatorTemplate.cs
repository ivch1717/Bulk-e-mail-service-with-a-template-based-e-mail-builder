using ConstructorUseCases.ExportTemplate;

namespace ConstructureInfrastructure.Data.Validators;

public class HtmlValidatorTemplate : IHtmlValidatorTemplate
{
    public bool IsValid(string html)
    {
        return true;
    }
}