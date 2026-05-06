using Microsoft.EntityFrameworkCore;

namespace Publisher;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<OutboxEmail> OutboxEmails { get; set; }
}