using Entities;

namespace ConstructorUseCases.DeleteFileById;

public interface IDeleteFileByIdRepository
{
    bool ExistsById(Guid fileId);
    void DeleteFileById(Guid fileId);
}
