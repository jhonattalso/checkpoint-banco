using Microsoft.AspNetCore.Mvc;
using ProjetoBanco.API.Data;
using ProjetoBanco.API.DTOs;
using ProjetoBanco.API.Models;

namespace ProjetoBanco.API.Controllers;

[ApiController]
[Route("api/agencias")]
public class AgenciasController : ControllerBase {
    private readonly AppDbContext _db;
    private readonly ILogger<AgenciasController> _logger;

    public AgenciasController(AppDbContext db, ILogger<AgenciasController> logger) {
        _db = db; _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarAgenciaDto dto) {
        if (string.IsNullOrWhiteSpace(dto.Nome) ||
            string.IsNullOrWhiteSpace(dto.Numero) ||
            string.IsNullOrWhiteSpace(dto.Endereco))
            return BadRequest("Nome, Número e Endereço são obrigatórios.");

        var agencia = new Agencia { Nome = dto.Nome, Numero = dto.Numero, Endereco = dto.Endereco };
        _db.Agencias.Add(agencia);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Agência {Numero} cadastrada Id {Id}", agencia.Numero, agencia.Id);
        return CreatedAtAction(nameof(BuscarPorId), new { id = agencia.Id },
            new AgenciaResponseDto(agencia.Id, agencia.Nome, agencia.Numero, agencia.Endereco));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id) {
        var agencia = await _db.Agencias.FindAsync(id);
        if (agencia == null) return NotFound($"Agência {id} não encontrada.");
        return Ok(new AgenciaResponseDto(agencia.Id, agencia.Nome, agencia.Numero, agencia.Endereco));
    }
}