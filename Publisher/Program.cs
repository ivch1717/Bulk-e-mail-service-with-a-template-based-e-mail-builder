using Microsoft.EntityFrameworkCore;

namespace Publisher;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();
        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Outbox")));
        var host = builder.Build();
        host.Run();
    }
}