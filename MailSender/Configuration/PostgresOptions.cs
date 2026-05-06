namespace MailSender;

public sealed class PostgresOptions
{
    public string ConnectionString { get; init; } =
        "Host=postgres-mail-service;Port=5432;Database=mailservice;Username=postgres;Password=postgres";
}
