using System.ComponentModel.DataAnnotations;

namespace ConstructorUseCases.ImportBlock;

public class ImportBlockRequestHandler : IImportBlockRequestHandler
{
    public ImportBlockResponse Handle(ImportBlockRequest request)
    {
        string html = request.Html;
        var openStart = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);

        if (openStart < 0)
        {
            return new ImportBlockResponse(html);
        }
        
        var openEnd = html.IndexOf('>', openStart);
        if (openEnd < 0)
        {
            throw new ValidationException("Invalid <body> tag");
        }

        var closeStart = html.IndexOf("</body>", openEnd + 1, StringComparison.OrdinalIgnoreCase);

        if (closeStart < 0)
        {
            throw new ValidationException("Invalid html");
        }
        
        return new ImportBlockResponse(html.Substring(openEnd + 1, closeStart - (openEnd + 1)));
    }
}