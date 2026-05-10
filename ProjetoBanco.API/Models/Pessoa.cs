using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoBanco.API.Models;

public class PessoaFisica : Cliente {
    [Required]
    [MaxLength(14)]
    [Column("CPF")]
    public string Cpf { get; set; } = string.Empty;

    [Column("DATA_NASCIMENTO")]
    public DateTime DataNascimento { get; set; }
}

public class PessoaJuridica : Cliente {
    [Required]
    [MaxLength(18)]
    [Column("CNPJ")]
    public string Cnpj { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("RAZAO_SOCIAL")]
    public string RazaoSocial { get; set; } = string.Empty;
}