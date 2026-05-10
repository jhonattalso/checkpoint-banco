using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjetoBanco.API.Migrations
{
    /// <inheritdoc />
    public partial class cp5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AGENCIAS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    NUMERO = table.Column<string>(type: "NVARCHAR2(8)", maxLength: 8, nullable: false),
                    ENDERECO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AGENCIAS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PRODUTOS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DESCRICAO = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    PRODUTO_TIPO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    PRODUTO_TIPO1 = table.Column<string>(type: "NVARCHAR2(13)", maxLength: 13, nullable: false),
                    VALOR_BEM = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    TOTAL_PARCELAS = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TAXA_ADMINISTRACAO = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    CATEGORIA = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    VALOR_SOLICITADO = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    PARCELAS = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TAXA_JUROS = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    SCORE_MINIMO = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUTOS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CLIENTES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    TELEFONE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    AGENCIA_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DISCRIMINATOR = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: true),
                    DISCRIMINATOR1 = table.Column<string>(type: "NVARCHAR2(8)", maxLength: 8, nullable: false),
                    CPF = table.Column<string>(type: "NVARCHAR2(14)", maxLength: 14, nullable: true),
                    DATA_NASCIMENTO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CNPJ = table.Column<string>(type: "NVARCHAR2(18)", maxLength: 18, nullable: true),
                    RAZAO_SOCIAL = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLIENTES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CLIENTES_AGENCIAS_AGENCIA_ID",
                        column: x => x.AGENCIA_ID,
                        principalTable: "AGENCIAS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CONTRATACOES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CLIENTE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PRODUTO_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    STATUS = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    DATA_SOLICITACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DATA_PROCESSAMENTO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    MOTIVO_RECUSA = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    PARCELA_MENSAL = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    VALOR_PAGO = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    SALDO_RESTANTE = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    PARCELA_ATUAL = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    NUMERO_GRUPO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    SCORE_CREDITO = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    VALOR_TOTAL_COM_JUROS = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONTRATACOES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CONTRATACOES_CLIENTES_CLIENTE_ID",
                        column: x => x.CLIENTE_ID,
                        principalTable: "CLIENTES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CONTRATACOES_PRODUTOS_PRODUTO_ID",
                        column: x => x.PRODUTO_ID,
                        principalTable: "PRODUTOS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "PRODUTOS",
                columns: new[] { "ID", "CATEGORIA", "DESCRICAO", "NOME", "PRODUTO_TIPO1", "PRODUTO_TIPO", "TAXA_ADMINISTRACAO", "TOTAL_PARCELAS", "VALOR_BEM" },
                values: new object[,]
                {
                    { 1, "IMOVEL", "Consórcio para aquisição de imóvel residencial", "Consórcio Imóvel", "CONSORCIO", "CONSORCIO", 1.2m, 200, 300000m },
                    { 2, "VEICULO", "Consórcio para aquisição de veículo", "Consórcio Veículo", "CONSORCIO", "CONSORCIO", 1.5m, 60, 80000m }
                });

            migrationBuilder.InsertData(
                table: "PRODUTOS",
                columns: new[] { "ID", "DESCRICAO", "NOME", "PRODUTO_TIPO1", "PARCELAS", "PRODUTO_TIPO", "SCORE_MINIMO", "TAXA_JUROS", "VALOR_SOLICITADO" },
                values: new object[] { 3, "Empréstimo pessoal com análise de score de crédito", "Empréstimo Pessoal", "EMPRESTIMO", 24, "EMPRESTIMO", 600, 2.5m, 10000m });

            migrationBuilder.CreateIndex(
                name: "IX_CLIENTES_AGENCIA_ID",
                table: "CLIENTES",
                column: "AGENCIA_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENTES_CNPJ",
                table: "CLIENTES",
                column: "CNPJ",
                unique: true,
                filter: "\"CNPJ\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CLIENTES_CPF",
                table: "CLIENTES",
                column: "CPF",
                unique: true,
                filter: "\"CPF\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CONTRATACOES_CLIENTE_ID",
                table: "CONTRATACOES",
                column: "CLIENTE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CONTRATACOES_PRODUTO_ID",
                table: "CONTRATACOES",
                column: "PRODUTO_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONTRATACOES");

            migrationBuilder.DropTable(
                name: "CLIENTES");

            migrationBuilder.DropTable(
                name: "PRODUTOS");

            migrationBuilder.DropTable(
                name: "AGENCIAS");
        }
    }
}
