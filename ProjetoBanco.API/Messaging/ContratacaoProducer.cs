using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace ProjetoBanco.API.Messaging;

public interface IContratacaoProducer {
    void Publicar(ContratacaoMessage mensagem);
}

public class ContratacaoProducer : IContratacaoProducer, IDisposable {
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string QueueName = "contratacao-solicitada";

    public ContratacaoProducer(IConfiguration configuration) {
        var factory = new ConnectionFactory {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:User"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(
            queue: QueueName, durable: true,
            exclusive: false, autoDelete: false, arguments: null);
    }

    public void Publicar(ContratacaoMessage mensagem) {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(mensagem));
        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        _channel.BasicPublish(
            exchange: string.Empty, routingKey: QueueName,
            basicProperties: props, body: body);
    }

    public void Dispose() {
        _channel?.Close();
        _connection?.Close();
    }
}