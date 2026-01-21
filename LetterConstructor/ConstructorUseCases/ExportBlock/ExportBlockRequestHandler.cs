using System.ComponentModel.DataAnnotations;
using System.Text;
using ConstructorUseCases.Common;

namespace ConstructorUseCases.ExportBlock;

public class ExportBlockRequestHandler : IExportBlockRequestHandler
{
    public IHtmlValidatorBody _htmlValidatorBody;

    public ExportBlockRequestHandler(IHtmlValidatorBody htmlValidatorBody)
    {
        _htmlValidatorBody = htmlValidatorBody;
    }
    
    public ExportBlockResponse Handle(ExportBlockRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Html))
        {
            throw new ValidationException("Invalid html");
        }
        if (!_htmlValidatorBody.IsValid(request.Html))
        {
            throw new ValidationException("Invalid html");
        }
        
        var bytes = Encoding.UTF8.GetBytes(request.Html);
        var stream = new MemoryStream(bytes);

        return new ExportBlockResponse(
            Content: stream,
            FileName: "block.html",
            ContentType: "text/html; charset=utf-8"
        );
    }
}