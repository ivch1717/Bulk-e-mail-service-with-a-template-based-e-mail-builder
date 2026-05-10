using Entities;

namespace ConstructorUseCases.AddFile;

public static class AddFileMapper
{
    public static HtmlFile ToEntity(AddFileRequest request, Guid fileId)
    {
        return new HtmlFile(fileId, request.Name, request.Content, request.CreatedAt, request.Type);
    }
}
