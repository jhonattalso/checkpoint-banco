using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoBanco.API.Models;

[Table("CONTRATACOES")]
public class Contratacao {
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CLIENTE_ID")]
    public int ClienteId { get; set; }

    [ForeignKey("ClienteId")]
    public Cliente? Cliente { get; set; }

    [Column("PRODUTO_ID")]
    public int ProdutoId { get; set; }

    [ForeignKey("ProdutoId")]
    public Produto? Produto { get; set; }

    [Column("STATUS")]
    [MaxLength(20)]
    public string Status { get; set; } = StatusContratacao.Pendente;

    [Column("DATA_SOLICITACAO")]
    public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;

    [Column("DATA_PROCESSAMENTO")]
    public DateTime? DataProcessamento { get; set; }

    [Column("MOTIVO_RECUSA")]
    [MaxLength(500)]
    public string? MotivoRecusa { get; set; }

    [Column("PARCELA_MENSAL")]
    public decimal? ParcelaMensal { get; set; }

    [Column("VALOR_PAGO")]
    public decimal? ValorPago { get; set; }

    [Column("SALDO_RESTANTE")]
    public decimal? SaldoRestante { get; set; }

    [Column("PARCELA_ATUAL")]
    public int? ParcelaAtual { get; set; }

    [Column("NUMERO_GRUPO")]
    [MaxLength(20)]
    public string? NumeroGrupo { get; set; }

    [Column("SCORE_CREDITO")]
    public int? ScoreCredito { get; set; }

    [Column("VALOR_TOTAL_COM_JUROS")]
    public decimal? ValorTotalComJuros { get; set; }
}

public static class StatusContratacao {
    public const string Pendente = "PENDENTE";
    public const string Aprovada = "APROVADA";
    public const string Recusada = "RECUSADA";
}