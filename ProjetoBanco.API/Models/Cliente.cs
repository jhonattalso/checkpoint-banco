using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoBanco.API.Models;

[Table("CLIENTES")]
public abstract class Cliente {
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("NOME")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("EMAIL")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("TELEFONE")]
    public string Telefone { get; set; } = string.Empty;

    [Column("AGENCIA_ID")]
    public int AgenciaId { get; set; }

    [ForeignKey("AgenciaId")]
    public Agencia? Agencia { get; set; }

    [Column("DISCRIMINATOR")]
    [MaxLength(2)]
    public string Discriminator { get; set; } = string.Empty;

    public ICollection<Contratacao> Contratacoes { get; set; } = new List<Contratacao>();
}