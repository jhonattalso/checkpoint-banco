using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoBanco.API.Data;
using ProjetoBanco.API.DTOs;
using ProjetoBanco.API.Messaging;
using ProjetoBanco.API.Models;

namespace ProjetoBanco.API.Controllers;

[ApiController]
[Route("api/contratacoes")]
public class ContratacoesController : ControllerBase {
    private readonly AppDbContext _db;
    private readonly IContratacaoProducer _producer;
    private readonly ILogger<ContratacoesController> _logger;

    public ContratacoesController(
        AppDbContext db, IContratacaoProducer producer,
        ILogger<ContratacoesController> logger) {
        _db = db; _producer = producer; _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Solicitar([FromBody] CriarContratacaoDto dto) {
        var cliente = await _db.Clientes.FindAsync(dto.ClienteId);
        if (cliente == null) return NotFound($"Cliente {dto.ClienteId} não encontrado.");

        var produto = await _db.Produtos.FindAsync(dto.ProdutoId);
        if (produto == null) return NotFound($"Produto {dto.ProdutoId} não encontrado.");

        var contratacao = new Contratacao {
            ClienteId = dto.ClienteId,
            ProdutoId = dto.ProdutoId,
            Status = StatusContratacao.Pendente,
            DataSolicitacao = DateTime.UtcNow
        };
        _db.Contratacoes.Add(contratacao);
        await _db.SaveChangesAsync();

        _producer.Publicar(new ContratacaoMessage(
            contratacao.Id, dto.ClienteId, dto.ProdutoId,
            produto.ProdutoTipo, DateTime.UtcNow));

        _logger.LogInformation("Contratação {Id} PENDENTE publicada na fila. Tipo: {Tipo}",
            contratacao.Id, produto.ProdutoTipo);

        return AcceptedAtAction(nameof(BuscarPorId), new { id = contratacao.Id },
            Mapear(contratacao, cliente, produto));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id) {
        var c = await _db.Contratacoes
            .Include(x => x.Cliente)
            .Include(x => x.Produto)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound($"Contratação {id} não encontrada.");
        return Ok(Mapear(c, c.Cliente!, c.Produto!));
    }

    private static ContratacaoResponseDto Mapear(Contratacao c, Cliente cliente, Produto produto) =>
        new(c.Id, c.ClienteId, cliente.Nome,
            c.ProdutoId, produto.Nome, produto.ProdutoTipo,
            c.Status, c.DataSolicitacao, c.DataProcessamento, c.MotivoRecusa,
            c.ParcelaMensal, c.ValorPago, c.SaldoRestante, c.ParcelaAtual,
            (produto as Consorcio)?.TotalParcelas,
            c.NumeroGrupo, (produto as Consorcio)?.Categoria,
            c.ScoreCredito, c.ValorTotalComJuros);
}