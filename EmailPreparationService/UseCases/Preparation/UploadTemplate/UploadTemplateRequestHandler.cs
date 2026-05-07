using System.Text.RegularExpressions;
using UseCases.TemplateUtilities;

namespace UseCases.Preparation.UploadTemplate;

/// <summary>
/// Обработчик шаблона.
/// </summary>
/// <param name="templateFactory">Фабрика шаблонов.</param>
public partial class UploadTemplateRequestHandler(ITemplateFactory templateFactory) : IUploadTemplateRequestHandler
{
    /// <summary>
    /// Распознает переменные в шаблоне.
    /// </summary>
    /// <param name="request">Шаблон.</param>
    /// <returns>Переменные.</returns>
    public UploadTemplateResponse Handle(UploadTemplateRequest request)
    {
        var template = templateFactory.Create(request.template, false);
        HashSet<string> variables = ["email"];
        var templateVariables = GetVariables().Matches(template.ToString()!);
        
        foreach (var variable in templateVariables)
        {
            var match = variable.ToString()!;
            variables.Add(match[2..^2]);
        }
        
        return new UploadTemplateResponse(variables.ToList());
    }

    [GeneratedRegex(@"\[\[(.*?)\]\]")]
    private static partial Regex GetVariables();
}