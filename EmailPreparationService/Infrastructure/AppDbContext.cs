using Microsoft.EntityFrameworkCore;
using Models;

namespace Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<OutboxEmail> OutboxEmails { get; set; }
    public DbSet<EmailOpenData> EmailOpenDatas { get; set; }
    public DbSet<SmtpProfile> SmtpProfiles { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
}