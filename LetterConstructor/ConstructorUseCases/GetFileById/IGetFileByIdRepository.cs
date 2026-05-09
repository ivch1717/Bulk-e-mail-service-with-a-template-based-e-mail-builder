using Entities;

namespace ConstructorUseCases.GetFileById;

public interface IGetFileByIdRepository
{
    HtmlFile? GetFileById(Guid fileId);
}
