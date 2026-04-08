using Microsoft.EntityFrameworkCore;
using Models;

namespace Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<OutboxEmail> OutboxEmails { get; set; }
}