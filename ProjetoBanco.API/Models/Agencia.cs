using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoBanco.API.Models;

[Table("AGENCIAS")]
public class Agencia {
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("NOME")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(8)]
    [Column("NUMERO")]
    public string Numero { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("ENDERECO")]
    public string Endereco { get; set; } = string.Empty;

    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}