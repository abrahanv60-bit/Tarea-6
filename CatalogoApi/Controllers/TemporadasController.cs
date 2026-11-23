using CatalogoApi.Data;
using CatalogoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/temporadas")]
[Authorize]
public class TemporadasController : ControllerBase
{
    private readonly AppDbContext _context;

    public TemporadasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var temporadas = await _context.Temporadas
            .Where(t => t.Activo)
            .ToListAsync();
        return Ok(temporadas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var temporada = await _context.Temporadas
            .FirstOrDefaultAsync(t => t.Id == id && t.Activo);

        if (temporada == null)
        {
            return NotFound(new { mensaje = $"Temporada con ID {id} no encontrada." });
        }
        return Ok(temporada);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Temporada temporada)
    {
        if (string.IsNullOrWhiteSpace(temporada.Titulo))
        {
            return BadRequest(new { mensaje = "El título de la temporada es obligatorio." });
        }

        var existeSerie = await _context.Series
            .AnyAsync(s => s.Id == temporada.SerieId && s.Activo);
        if (!existeSerie)
        {
            return BadRequest(new { mensaje = $"La serie con ID {temporada.SerieId} no existe o está inactiva." });
        }

        temporada.CreadoEn = DateTime.UtcNow;
        _context.Temporadas.Add(temporada);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = temporada.Id }, temporada);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Temporada temporada)
    {
        if (id != temporada.Id)
        {
            return BadRequest(new { mensaje = "El ID de la ruta no coincide con el ID del cuerpo." });
        }

        if (string.IsNullOrWhiteSpace(temporada.Titulo))
        {
            return BadRequest(new { mensaje = "El título de la temporada es obligatorio." });
        }

        var existing = await _context.Temporadas.FindAsync(id);
        if (existing == null || !existing.Activo)
        {
            return NotFound(new { mensaje = $"Temporada con ID {id} no existe o no se encuentra activa." });
        }

        var existeSerie = await _context.Series
            .AnyAsync(s => s.Id == temporada.SerieId && s.Activo);
        if (!existeSerie)
        {
            return BadRequest(new { mensaje = $"La serie con ID {temporada.SerieId} no existe o está inactiva." });
        }

        existing.Numero = temporada.Numero;
        existing.Titulo = temporada.Titulo;
        existing.AnioEstreno = temporada.AnioEstreno;
        existing.CantidadEpisodios = temporada.CantidadEpisodios;
        existing.SerieId = temporada.SerieId;
        existing.Activo = temporada.Activo;
        if (!temporada.Activo && existing.EliminadoEn == null)
        {
            existing.EliminadoEn = temporada.EliminadoEn ?? DateTime.UtcNow;
        }
        else if (temporada.Activo)
        {
            existing.EliminadoEn = null;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var temporada = await _context.Temporadas.FindAsync(id);
        if (temporada == null)
        {
            return NotFound(new { mensaje = $"Temporada con ID {id} no encontrada." });
        }

        _context.Temporadas.Remove(temporada);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}