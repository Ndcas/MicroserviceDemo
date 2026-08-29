using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;

namespace user.Pulsar
{
    public interface ILogProducer
    {
        Task Send(string correlationId, string ip, string content, LogLevel level = LogLevel.Information);
    }

    public class LogProducer : ILogProducer, IAsyncDisposable
    {
        private IProducer<string> _producer;

        public LogProducer(IPulsarClient client)
        {
            _producer = client.NewProducer(Schema.String).Topic("Log").Create();
        }

        public async Task Send(string correlationId, string ip, string content, LogLevel level = LogLevel.Information)
        {
            await _producer.NewMessage().Property("source", "UserService").Property("correlationId", correlationId).Property("ip", ip)
                .Property("level", level.ToString()).Property("time", DateTime.Now.ToString()).Send(content);
        }

        public ValueTask DisposeAsync()
        {
            return _producer.DisposeAsync();
        }
    }
}
