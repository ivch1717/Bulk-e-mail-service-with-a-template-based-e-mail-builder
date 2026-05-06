namespace MailSender;

public sealed class PostgresOptions
{
    public string ConnectionString { get; init; } =
        "Host=postgres-mail-sender;Port=5432;Database=mailsender;Username=postgres;Password=postgres";
}
