using Microsoft.EntityFrameworkCore;
using ProjetoBanco.API.Models;

namespace ProjetoBanco.API.Data;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<PessoaFisica> PessoasFisicas { get; set; }
    public DbSet<PessoaJuridica> PessoasJuridicas { get; set; }
    public DbSet<Agencia> Agencias { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Consorcio> Consorcios { get; set; }
    public DbSet<Emprestimo> Emprestimos { get; set; }
    public DbSet<Contratacao> Contratacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>()
            .HasDiscriminator<string>("DISCRIMINATOR")
            .HasValue<PessoaFisica>("PF")
            .HasValue<PessoaJuridica>("PJ");

        modelBuilder.Entity<Produto>()
            .HasDiscriminator<string>("PRODUTO_TIPO")
            .HasValue<Consorcio>("CONSORCIO")
            .HasValue<Emprestimo>("EMPRESTIMO");

        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Agencia)
            .WithMany(a => a.Clientes)
            .HasForeignKey(c => c.AgenciaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Contratacao>()
            .HasOne(c => c.Cliente)
            .WithMany(cl => cl.Contratacoes)
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Contratacao>()
            .HasOne(c => c.Produto)
            .WithMany()
            .HasForeignKey(c => c.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PessoaFisica>()
            .HasIndex(p => p.Cpf)
            .IsUnique();

        modelBuilder.Entity<PessoaJuridica>()
            .HasIndex(p => p.Cnpj)
            .IsUnique();

        modelBuilder.Entity<Consorcio>().HasData(new Consorcio {
            Id = 1,
            Nome = "Consórcio Imóvel",
            Descricao = "Consórcio para aquisição de imóvel residencial",
            ProdutoTipo = "CONSORCIO",
            ValorBem = 300000m,
            TotalParcelas = 200,
            TaxaAdministracao = 1.2m,
            Categoria = "IMOVEL"
        });

        modelBuilder.Entity<Consorcio>().HasData(new Consorcio {
            Id = 2,
            Nome = "Consórcio Veículo",
            Descricao = "Consórcio para aquisição de veículo",
            ProdutoTipo = "CONSORCIO",
            ValorBem = 80000m,
            TotalParcelas = 60,
            TaxaAdministracao = 1.5m,
            Categoria = "VEICULO"
        });

        modelBuilder.Entity<Emprestimo>().HasData(new Emprestimo {
            Id = 3,
            Nome = "Empréstimo Pessoal",
            Descricao = "Empréstimo pessoal com análise de score de crédito",
            ProdutoTipo = "EMPRESTIMO",
            ValorSolicitado = 10000m,
            Parcelas = 24,
            TaxaJuros = 2.5m,
            ScoreMinimo = 600
        });
    }
}