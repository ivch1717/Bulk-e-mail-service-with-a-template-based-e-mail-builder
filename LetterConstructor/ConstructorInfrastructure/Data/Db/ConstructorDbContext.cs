using ConstructureInfrastructure.Data.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ConstructureInfrastructure.Data.Db;

public sealed class ConstructorDbContext(DbContextOptions<ConstructorDbContext> options) : DbContext(options)
{
    public DbSet<HtmlFileDto> HtmlFiles => Set<HtmlFileDto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HtmlFileDto>(builder =>
        {
            builder.ToTable("HtmlFiles");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Content).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            
            builder.HasIndex(x => x.Name).IsUnique();
        });
    }
}
