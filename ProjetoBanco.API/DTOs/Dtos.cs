namespace ProjetoBanco.API.DTOs;

public record CriarAgenciaDto(string Nome, string Numero, string Endereco);

public record AgenciaResponseDto(int Id, string Nome, string Numero, string Endereco);

public record CriarPessoaFisicaDto(
    string Nome, string Email, string Telefone,
    string Cpf, DateTime DataNascimento, int AgenciaId);

public record CriarPessoaJuridicaDto(
    string Nome, string Email, string Telefone,
    string Cnpj, string RazaoSocial, int AgenciaId);

public record ClienteResponseDto(
    int Id, string Nome, string Email, string Telefone,
    string Tipo, string? Cpf, DateTime? DataNascimento,
    string? Cnpj, string? RazaoSocial,
    int AgenciaId, string? NomeAgencia);

public record CriarContratacaoDto(int ClienteId, int ProdutoId);

public record ContratacaoResponseDto(
    int Id, int ClienteId, string NomeCliente,
    int ProdutoId, string NomeProduto, string TipoProduto,
    string Status, DateTime DataSolicitacao,
    DateTime? DataProcessamento, string? MotivoRecusa,
    decimal? ParcelaMensal, decimal? ValorPago,
    decimal? SaldoRestante, int? ParcelaAtual,
    int? TotalParcelas, string? NumeroGrupo, string? Categoria,
    int? ScoreCredito, decimal? ValorTotalComJuros);