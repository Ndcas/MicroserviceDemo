using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using log.Services;

namespace log.Pulsar
{
    public class LogConsumer : BackgroundService
    {
        private IPulsarClient _client;
        private IServiceScopeFactory _serviceScopeFactory;

        public LogConsumer(IPulsarClient client, IServiceScopeFactory serviceScopeFactory)
        {
            _client = client;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using IConsumer<string> consumer = _client.NewConsumer(Schema.String).SubscriptionName("LogServiceSubscription")
                .Topic("Log").Create();

            await foreach (IMessage<string> message in consumer.Messages(stoppingToken))
            {
                try
                {
                    using IServiceScope scope = _serviceScopeFactory.CreateScope();
                    ILogService logService = scope.ServiceProvider.GetRequiredService<ILogService>();
                    string level = message.Properties["level"];
                    string source = message.Properties["source"];
                    string correlationId = message.Properties["correlationId"];
                    string ip = message.Properties["ip"];
                    string time = message.Properties["time"];
                    string content = message.Value();

                    await logService.WriteLog(level, source, correlationId, ip, time, content);

                    await consumer.Acknowledge(message, stoppingToken);
                }
                catch
                {
                    await consumer.RedeliverUnacknowledgedMessages(new[] { message.MessageId }, stoppingToken);
                }
            }
        }
    }
}
