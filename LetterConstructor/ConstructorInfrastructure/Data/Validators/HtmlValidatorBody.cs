using ConstructorUseCases.Common;

namespace ConstructureInfrastructure.Data.Validators;

public class HtmlValidatorBody : IHtmlValidatorBody
{
    public bool IsValid(string html)
    {
        return true;
    }
}