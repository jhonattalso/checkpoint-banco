using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoBanco.API.Data;
using ProjetoBanco.API.DTOs;
using ProjetoBanco.API.Models;

namespace ProjetoBanco.API.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase {
    private readonly AppDbContext _db;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(AppDbContext db, ILogger<ClientesController> logger) {
        _db = db; _logger = logger;
    }

    [HttpPost("pf")]
    public async Task<IActionResult> CriarPF([FromBody] CriarPessoaFisicaDto dto) {
        var agencia = await _db.Agencias.FindAsync(dto.AgenciaId);
        if (agencia == null) return NotFound($"Agência {dto.AgenciaId} não encontrada.");

        var existe = await _db.PessoasFisicas.FirstOrDefaultAsync(p => p.Cpf == dto.Cpf);
        if (existe != null) return BadRequest($"CPF {dto.Cpf} já cadastrado.");

        var pf = new PessoaFisica {
            Nome = dto.Nome,
            Email = dto.Email,
            Telefone = dto.Telefone,
            Cpf = dto.Cpf,
            DataNascimento = dto.DataNascimento,
            AgenciaId = dto.AgenciaId,
            Discriminator = "PF"
        };
        _db.PessoasFisicas.Add(pf);
        await _db.SaveChangesAsync();

        _logger.LogInformation("PF {Nome} CPF {Cpf} Id {Id}", pf.Nome, pf.Cpf, pf.Id);
        return CreatedAtAction(nameof(BuscarPorId), new { id = pf.Id }, Mapear(pf, agencia));
    }

    [HttpPost("pj")]
    public async Task<IActionResult> CriarPJ([FromBody] CriarPessoaJuridicaDto dto) {
        var agencia = await _db.Agencias.FindAsync(dto.AgenciaId);
        if (agencia == null) return NotFound($"Agência {dto.AgenciaId} não encontrada.");

        var existe = await _db.PessoasJuridicas.FirstOrDefaultAsync(p => p.Cnpj == dto.Cnpj);
        if (existe != null) return BadRequest($"CNPJ {dto.Cnpj} já cadastrado.");

        var pj = new PessoaJuridica {
            Nome = dto.Nome,
            Email = dto.Email,
            Telefone = dto.Telefone,
            Cnpj = dto.Cnpj,
            RazaoSocial = dto.RazaoSocial,
            AgenciaId = dto.AgenciaId,
            Discriminator = "PJ"
        };
        _db.PessoasJuridicas.Add(pj);
        await _db.SaveChangesAsync();

        _logger.LogInformation("PJ {RazaoSocial} CNPJ {Cnpj} Id {Id}", pj.RazaoSocial, pj.Cnpj, pj.Id);
        return CreatedAtAction(nameof(BuscarPorId), new { id = pj.Id }, Mapear(pj, agencia));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id) {
        var cliente = await _db.Clientes.Include(c => c.Agencia)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (cliente == null) return NotFound($"Cliente {id} não encontrado.");
        return Ok(Mapear(cliente, cliente.Agencia!));
    }

    private static ClienteResponseDto Mapear(Cliente c, Agencia a) => c switch {
        PessoaFisica pf => new(pf.Id, pf.Nome, pf.Email, pf.Telefone,
            "PF", pf.Cpf, pf.DataNascimento, null, null, pf.AgenciaId, a.Nome),
        PessoaJuridica pj => new(pj.Id, pj.Nome, pj.Email, pj.Telefone,
            "PJ", null, null, pj.Cnpj, pj.RazaoSocial, pj.AgenciaId, a.Nome),
        _ => throw new InvalidOperationException("Tipo desconhecido.")
    };
}