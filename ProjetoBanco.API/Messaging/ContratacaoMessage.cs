namespace ProjetoBanco.API.Messaging;

public record ContratacaoMessage(
    int ContratacaoId,
    int ClienteId,
    int ProdutoId,
    string TipoProduto,
    DateTime DataEnvio
);