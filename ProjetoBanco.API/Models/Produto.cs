using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoBanco.API.Models;

[Table("PRODUTOS")]
public abstract class Produto {
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("NOME")]
    public string Nome { get; set; } = string.Empty;

    [Column("DESCRICAO")]
    [MaxLength(500)]
    public string Descricao { get; set; } = string.Empty;

    [Column("PRODUTO_TIPO")]
    [MaxLength(20)]
    public string ProdutoTipo { get; set; } = string.Empty;
}

public class Consorcio : Produto {
    [Column("VALOR_BEM")]
    public decimal ValorBem { get; set; }

    [Column("TOTAL_PARCELAS")]
    public int TotalParcelas { get; set; }

    [Column("TAXA_ADMINISTRACAO")]
    public decimal TaxaAdministracao { get; set; }

    [Column("CATEGORIA")]
    [MaxLength(20)]
    public string Categoria { get; set; } = string.Empty;
}

public class Emprestimo : Produto {
    [Column("VALOR_SOLICITADO")]
    public decimal ValorSolicitado { get; set; }

    [Column("PARCELAS")]
    public int Parcelas { get; set; }

    [Column("TAXA_JUROS")]
    public decimal TaxaJuros { get; set; }

    [Column("SCORE_MINIMO")]
    public int ScoreMinimo { get; set; }
}