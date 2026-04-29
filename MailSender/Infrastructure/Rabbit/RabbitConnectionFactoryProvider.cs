using RabbitMQ.Client;

namespace MailSender;

internal static class RabbitConnectionFactoryProvider
{
    public static ConnectionFactory Create(RabbitOptions options) =>
        new()
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.User,
            Password = options.Pass,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
}
