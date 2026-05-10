using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ProjetoBanco.API.Data;
using ProjetoBanco.API.DTOs;
using ProjetoBanco.API.Messaging;
using ProjetoBanco.API.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ProjetoBanco.Tests;

public class ProjetoBancoFactory : WebApplicationFactory<Program> {
    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services => {
            var desc = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (desc != null) services.Remove(desc);

            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("TestDb"));
            services.AddSingleton<IContratacaoProducer>(
                new Mock<IContratacaoProducer>().Object);
        });
    }
}

public class AgenciaTests : IClassFixture<ProjetoBancoFactory> {
    private readonly HttpClient _client;
    public AgenciaTests(ProjetoBancoFactory f) => _client = f.CreateClient();

    [Fact]
    public async Task CriarAgencia_DeveRetornar201() {
        var r = await _client.PostAsJsonAsync("/api/agencias",
            new CriarAgenciaDto("Ag. Centro", "0001", "Rua A, 100"));
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
    }

    [Fact]
    public async Task BuscarAgenciaInexistente_DeveRetornar404() {
        var r = await _client.GetAsync("/api/agencias/999999");
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }
}

public class ClienteTests : IClassFixture<ProjetoBancoFactory> {
    private readonly HttpClient _client;
    public ClienteTests(ProjetoBancoFactory f) => _client = f.CreateClient();

    private async Task<int> CriarAgencia(string num) {
        var r = await _client.PostAsJsonAsync("/api/agencias",
            new CriarAgenciaDto("Ag Teste", num, "Rua B"));
        var b = await r.Content.ReadFromJsonAsync<AgenciaResponseDto>();
        return b!.Id;
    }

    [Fact]
    public async Task CriarPF_DeveRetornar201() {
        var agId = await CriarAgencia("1001");
        var r = await _client.PostAsJsonAsync("/api/clientes/pf",
            new CriarPessoaFisicaDto("João", "joao@t.com", "11900000001",
                "111.111.111-01", new DateTime(1990, 1, 1), agId));
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
    }

    [Fact]
    public async Task CriarPF_CPFDuplicado_DeveRetornar400() {
        var agId = await CriarAgencia("1002");
        var dto = new CriarPessoaFisicaDto("Maria", "maria@t.com", "11900000002",
            "222.222.222-02", new DateTime(1990, 1, 1), agId);
        await _client.PostAsJsonAsync("/api/clientes/pf", dto);
        var r = await _client.PostAsJsonAsync("/api/clientes/pf", dto);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task CriarPJ_CNPJDuplicado_DeveRetornar400() {
        var agId = await CriarAgencia("1003");
        var dto = new CriarPessoaJuridicaDto("Empresa", "emp@t.com", "1133330001",
            "11.111.111/0001-11", "Empresa LTDA", agId);
        await _client.PostAsJsonAsync("/api/clientes/pj", dto);
        var r = await _client.PostAsJsonAsync("/api/clientes/pj", dto);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task CriarCliente_AgenciaInexistente_DeveRetornar404() {
        var r = await _client.PostAsJsonAsync("/api/clientes/pf",
            new CriarPessoaFisicaDto("Pedro", "pedro@t.com", "11900000003",
                "333.333.333-03", new DateTime(1990, 1, 1), 999999));
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    [Fact]
    public async Task BuscarClienteInexistente_DeveRetornar404() {
        var r = await _client.GetAsync("/api/clientes/999999");
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }
}

public class ContratacaoTests : IClassFixture<ProjetoBancoFactory> {
    private readonly HttpClient _client;
    private readonly ProjetoBancoFactory _factory;

    public ContratacaoTests(ProjetoBancoFactory f) {
        _factory = f; _client = f.CreateClient();
    }

    private async Task<int> CriarAgencia(string num) {
        var r = await _client.PostAsJsonAsync("/api/agencias",
            new CriarAgenciaDto("Ag Cont", num, "Rua C"));
        var b = await r.Content.ReadFromJsonAsync<AgenciaResponseDto>();
        return b!.Id;
    }

    private async Task<int> CriarCliente(int agId, string cpf) {
        var r = await _client.PostAsJsonAsync("/api/clientes/pf",
            new CriarPessoaFisicaDto("Cliente", $"{cpf}@t.com", "11900000000",
                cpf, new DateTime(1990, 1, 1), agId));
        var b = await r.Content.ReadFromJsonAsync<ClienteResponseDto>();
        return b!.Id;
    }

    private async Task<int> CriarProduto() {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var p = new Consorcio {
            Nome = "Consórcio Teste",
            Descricao = "Teste",
            ProdutoTipo = "CONSORCIO",
            ValorBem = 100000m,
            TotalParcelas = 100,
            TaxaAdministracao = 1m,
            Categoria = "IMOVEL"
        };
        db.Consorcios.Add(p);
        await db.SaveChangesAsync();
        return p.Id;
    }

    [Fact]
    public async Task SolicitarContratacao_Valida_DeveRetornar202() {
        var agId = await CriarAgencia("2001");
        var clienteId = await CriarCliente(agId, "444.444.444-04");
        var produtoId = await CriarProduto();

        var r = await _client.PostAsJsonAsync("/api/contratacoes",
            new CriarContratacaoDto(clienteId, produtoId));
        Assert.Equal(HttpStatusCode.Accepted, r.StatusCode);
    }

    [Fact]
    public async Task SolicitarContratacao_ClienteInexistente_DeveRetornar404() {
        var produtoId = await CriarProduto();
        var r = await _client.PostAsJsonAsync("/api/contratacoes",
            new CriarContratacaoDto(999999, produtoId));
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    [Fact]
    public async Task ConsultarContratacao_DeveRetornarStatus() {
        var agId = await CriarAgencia("2002");
        var clienteId = await CriarCliente(agId, "555.555.555-05");
        var produtoId = await CriarProduto();

        var post = await _client.PostAsJsonAsync("/api/contratacoes",
            new CriarContratacaoDto(clienteId, produtoId));
        var criada = await post.Content.ReadFromJsonAsync<ContratacaoResponseDto>();

        var get = await _client.GetAsync($"/api/contratacoes/{criada!.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task ConsultarContratacaoInexistente_DeveRetornar404() {
        var r = await _client.GetAsync("/api/contratacoes/999999");
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    [Fact]
    public async Task SolicitarContratacao_ProducerChamado_UmaVez() {
        var mock = new Mock<IContratacaoProducer>();
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => {
                b.UseEnvironment("Testing");
                b.ConfigureTestServices(services => {
                    var d = services.SingleOrDefault(
                        x => x.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (d != null) services.Remove(d);
                    services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("TestDbMock"));
                    services.AddSingleton<IContratacaoProducer>(mock.Object);
                });
            });

        var client = factory.CreateClient();

        var ag = await client.PostAsJsonAsync("/api/agencias",
            new CriarAgenciaDto("Ag Mock", "9001", "Rua Mock"));
        var agObj = await ag.Content.ReadFromJsonAsync<AgenciaResponseDto>();

        var cl = await client.PostAsJsonAsync("/api/clientes/pf",
            new CriarPessoaFisicaDto("Mock", "mock@t.com", "11900000002",
                "666.666.666-06", new DateTime(1990, 1, 1), agObj!.Id));
        var clObj = await cl.Content.ReadFromJsonAsync<ClienteResponseDto>();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var prod = new Consorcio {
            Nome = "Mock",
            Descricao = "Mock",
            ProdutoTipo = "CONSORCIO",
            ValorBem = 50000m,
            TotalParcelas = 50,
            TaxaAdministracao = 1m,
            Categoria = "IMOVEL"
        };
        db.Consorcios.Add(prod);
        await db.SaveChangesAsync();

        var r = await client.PostAsJsonAsync("/api/contratacoes",
            new CriarContratacaoDto(clObj!.Id, prod.Id));

        Assert.Equal(HttpStatusCode.Accepted, r.StatusCode);
        mock.Verify(p => p.Publicar(It.IsAny<ContratacaoMessage>()), Times.Once);
    }
}

public class IntegrationTests : IClassFixture<ProjetoBancoFactory> {
    private readonly HttpClient _client;
    public IntegrationTests(ProjetoBancoFactory f) => _client = f.CreateClient();

    [Fact]
    public async Task FluxoCompleto_AgenciaClienteHealthCheck() {
        var ag = await _client.PostAsJsonAsync("/api/agencias",
            new CriarAgenciaDto("Ag. Integração", "7777", "Av. Integração, 1000"));
        ag.EnsureSuccessStatusCode();
        var agObj = await ag.Content.ReadFromJsonAsync<AgenciaResponseDto>();

        var pf = await _client.PostAsJsonAsync("/api/clientes/pf",
            new CriarPessoaFisicaDto("Ana", "ana@t.com", "11988880001",
                "777.777.777-07", new DateTime(1995, 7, 15), agObj!.Id));
        pf.EnsureSuccessStatusCode();
        var pfObj = await pf.Content.ReadFromJsonAsync<ClienteResponseDto>();

        var get = await _client.GetAsync($"/api/clientes/{pfObj!.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var health = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
    }
}