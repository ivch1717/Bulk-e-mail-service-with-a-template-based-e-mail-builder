using ConstructorUseCases.GetAllFiles;
using ConstructureInfrastructure.Data.Db;

namespace ConstructureInfrastructure.Data.Repositories;

internal sealed class GetAllFilesRepository(ConstructorDbContext db) : IGetAllFilesRepository
{
    public List<FileSummary> GetAllFiles()
    {
        return db.HtmlFiles
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new FileSummary(x.Id, x.Name, x.CreatedAt))
            .ToList();
    }
}
