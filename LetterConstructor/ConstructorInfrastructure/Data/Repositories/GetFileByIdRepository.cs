using ConstructorUseCases.GetFileById;
using ConstructureInfrastructure.Data.Db;
using ConstructureInfrastructure.Data.Mappers;
using Entities;

namespace ConstructureInfrastructure.Data.Repositories;

internal sealed class GetFileByIdRepository(ConstructorDbContext db) : IGetFileByIdRepository
{
    public HtmlFile? GetFileById(Guid fileId)
    {
        return db.HtmlFiles
            .FirstOrDefault(x => x.Id == fileId)?
            .ToEntity();
    }
}
