using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProjetoBanco.API.Data;
using ProjetoBanco.API.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProjetoBanco.API.Messaging;

public class ContratacaoConsumer : BackgroundService {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ContratacaoConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private const string QueueName = "contratacao-solicitada";

    public ContratacaoConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<ContratacaoConsumer> logger) {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        ConectarRabbitMQ();
        var consumer = new EventingBasicConsumer(_channel!);

        consumer.Received += async (_, ea) => {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            ContratacaoMessage? msg = null;
            try {
                msg = JsonSerializer.Deserialize<ContratacaoMessage>(json);
                if (msg == null) {
                    _channel!.BasicNack(ea.DeliveryTag, false, false);
                    return;
                }

                _logger.LogInformation("Processando contratação {Id} tipo {Tipo}",
                    msg.ContratacaoId, msg.TipoProduto);

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var contratacao = await db.Contratacoes
                    .Include(c => c.Produto)
                    .FirstOrDefaultAsync(c => c.Id == msg.ContratacaoId, stoppingToken);

                if (contratacao == null) {
                    _channel!.BasicNack(ea.DeliveryTag, false, false);
                    return;
                }

                switch (msg.TipoProduto) {
                    case "CONSORCIO": ProcessarConsorcio(contratacao); break;
                    case "EMPRESTIMO": ProcessarEmprestimo(contratacao); break;
                    default:
                        contratacao.Status = StatusContratacao.Recusada;
                        contratacao.MotivoRecusa = "Tipo de produto não suportado.";
                        break;
                }

                contratacao.DataProcessamento = DateTime.UtcNow;
                await db.SaveChangesAsync(stoppingToken);

                // Delay para demonstração do Unacked no painel
                await Task.Delay(5000);
                // ACK manual — só confirma após salvar com sucesso
                _channel!.BasicAck(ea.DeliveryTag, false);

                _logger.LogInformation("Contratação {Id} → {Status}",
                    msg.ContratacaoId, contratacao.Status);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Erro ao processar contratação {Id}", msg?.ContratacaoId);
                _channel!.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
        };

        _channel!.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    private void ProcessarConsorcio(Contratacao contratacao) {
        var produto = contratacao.Produto as Consorcio;
        if (produto == null || produto.ValorBem <= 0 || produto.TotalParcelas <= 0) {
            contratacao.Status = StatusContratacao.Recusada;
            contratacao.MotivoRecusa = "Configuração do consórcio inválida.";
            return;
        }

        var parcelaMensal = produto.ValorBem / produto.TotalParcelas;
        var numeroGrupo = $"GRP-{new Random().Next(1000, 9999)}";

        contratacao.Status = StatusContratacao.Aprovada;
        contratacao.ParcelaMensal = Math.Round(parcelaMensal, 2);
        contratacao.ValorPago = 0m;
        contratacao.SaldoRestante = Math.Round(produto.ValorBem, 2);
        contratacao.ParcelaAtual = 1;
        contratacao.NumeroGrupo = numeroGrupo;

        _logger.LogInformation("Consórcio aprovado: Grupo {Grupo}, Parcela R$ {Parcela}",
            numeroGrupo, parcelaMensal);
    }

    private void ProcessarEmprestimo(Contratacao contratacao) {
        var produto = contratacao.Produto as Emprestimo;
        if (produto == null) {
            contratacao.Status = StatusContratacao.Recusada;
            contratacao.MotivoRecusa = "Produto não é um Empréstimo.";
            return;
        }

        var score = new Random(contratacao.ClienteId * 31 + DateTime.UtcNow.DayOfYear).Next(200, 851);
        contratacao.ScoreCredito = score;

        if (score < produto.ScoreMinimo) {
            contratacao.Status = StatusContratacao.Recusada;
            contratacao.MotivoRecusa = $"Score {score} abaixo do mínimo {produto.ScoreMinimo}.";
            return;
        }

        var taxa = produto.TaxaJuros / 100m;
        var n = produto.Parcelas;
        var fator = (double)(taxa * (decimal)Math.Pow((double)(1 + taxa), n))
                   / (double)((decimal)Math.Pow((double)(1 + taxa), n) - 1);
        var total = produto.ValorSolicitado * (decimal)fator * n;

        contratacao.Status = StatusContratacao.Aprovada;
        contratacao.ValorTotalComJuros = Math.Round(total, 2);
    }

    private void ConectarRabbitMQ() {
        var factory = new ConnectionFactory {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:User"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(
            queue: QueueName, durable: true,
            exclusive: false, autoDelete: false, arguments: null);
        _channel.BasicQos(0, 1, false);
    }

    public override void Dispose() {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}