using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ConstructorUseCases.ExportBlock;

public class ExportBlockRequestHandler : IExportBlockRequestHandler
{
    public IParseHtmlBock _parserHtmlBock;

    public ExportBlockRequestHandler(IParseHtmlBock parserHtmlBock)
    {
        _parserHtmlBock = parserHtmlBock;
    }
    
    public ExportBlockResponse Handle(ExportBlockRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Html))
        {
            throw new ValidationException("Invalid html");
        }
        
        /*string? html = _parserHtmlBock.Parse(request.Html);
        if (html is null)
        {
            throw new ValidationException("Invalid html");
        }
        
        if (string.IsNullOrWhiteSpace(html))
            throw new ValidationException("Parsed html is empty");*/

        
        var bytes = Encoding.UTF8.GetBytes(request.Html);
        var stream = new MemoryStream(bytes);
        stream.Position = 0;

        return new ExportBlockResponse(
            Content: stream,
            FileName: "block.html",
            ContentType: "text/html; charset=utf-8"
        );
    }
}