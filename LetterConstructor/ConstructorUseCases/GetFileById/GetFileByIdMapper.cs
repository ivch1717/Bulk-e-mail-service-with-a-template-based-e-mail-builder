using Entities;

namespace ConstructorUseCases.GetFileById;

public static class GetFileByIdMapper
{
    public static GetFileByIdResponse ToResponse(HtmlFile file)
    {
        return new GetFileByIdResponse(file.Id, file.Name, file.Content, file.CreatedAt);
    }
}
