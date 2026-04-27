namespace MailSender;

public sealed class PostgresOptions
{
    public string ConnectionString { get; init; } =
        "Host=localhost;Port=5432;Database=emailservice;Username=postgres;Password=postgres";
}
