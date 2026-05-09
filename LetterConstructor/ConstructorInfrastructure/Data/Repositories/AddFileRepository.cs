using ConstructorUseCases.AddFile;
using ConstructureInfrastructure.Data.Db;
using ConstructureInfrastructure.Data.Mappers;
using Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ConstructureInfrastructure.Data.Repositories;

internal sealed class AddFileRepository(ConstructorDbContext db) : IAddFileRepository
{
    private const string UniqueViolationSqlState = "23505";

    public void AddFile(HtmlFile file)
    {
        try
        {
            db.HtmlFiles.Add(file.ToDto());
            db.SaveChanges();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: UniqueViolationSqlState })
        {
            throw new FileNameAlreadyExistsException(file.Name);
        }
    }
}
