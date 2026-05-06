namespace MailSender;

public sealed class PostgresOptions
{
    public string ConnectionString { get; init; } =
        "Host=postgres;Port=5432;Database=emailservice;Username=postgres;Password=postgres";
}
