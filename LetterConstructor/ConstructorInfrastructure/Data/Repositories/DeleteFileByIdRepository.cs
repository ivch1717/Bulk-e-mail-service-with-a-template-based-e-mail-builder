using ConstructorUseCases.DeleteFileById;
using ConstructureInfrastructure.Data.Db;

namespace ConstructureInfrastructure.Data.Repositories;

internal sealed class DeleteFileByIdRepository(ConstructorDbContext db) : IDeleteFileByIdRepository
{
    public bool ExistsById(Guid fileId)
    {
        return db.HtmlFiles.Any(x => x.Id == fileId);
    }

    public void DeleteFileById(Guid fileId)
    {
        var file = db.HtmlFiles.FirstOrDefault(x => x.Id == fileId);

        if (file is null)
            return;

        db.HtmlFiles.Remove(file);
        db.SaveChanges();
    }
}
