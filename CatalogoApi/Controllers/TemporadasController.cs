using CatalogoApi.Data;
using CatalogoApi.Dtos;
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
    public async Task<ActionResult<IEnumerable<TemporadaResponseDto>>> GetAll()
    {
        var temporadas = await _context.Temporadas
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.SerieId)
            .ThenBy(t => t.Numero)
            .Select(t => new TemporadaResponseDto
            {
                Id = t.Id,
                Numero = t.Numero,
                Titulo = t.Titulo,
                AnioEstreno = t.AnioEstreno,
                CantidadEpisodios = t.CantidadEpisodios,
                SerieId = t.SerieId,
                SerieTitulo = t.Serie.Titulo,
                Activo = t.Activo,
                CreadoEn = t.CreadoEn,
                EliminadoEn = t.EliminadoEn
            })
            .ToListAsync();

        return Ok(temporadas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TemporadaResponseDto>> GetById(int id)
    {
        var temporada = await _context.Temporadas
            .AsNoTracking()
            .Where(t => t.Id == id && t.Activo)
            .Select(t => new TemporadaResponseDto
            {
                Id = t.Id,
                Numero = t.Numero,
                Titulo = t.Titulo,
                AnioEstreno = t.AnioEstreno,
                CantidadEpisodios = t.CantidadEpisodios,
                SerieId = t.SerieId,
                SerieTitulo = t.Serie.Titulo,
                Activo = t.Activo,
                CreadoEn = t.CreadoEn,
                EliminadoEn = t.EliminadoEn
            })
            .FirstOrDefaultAsync();

        if (temporada == null)
        {
            return NotFound(new { mensaje = $"Temporada con ID {id} no encontrada." });
        }

        return Ok(temporada);
    }

    [HttpPost]
    public async Task<ActionResult<TemporadaResponseDto>> Create([FromBody] TemporadaCreateDto dto)
    {
        var titulo = dto.Titulo.Trim();

        if (string.IsNullOrWhiteSpace(titulo))
        {
            return BadRequest(new { mensaje = "El título de la temporada es obligatorio." });
        }

        var serieTitulo = await _context.Series
            .Where(s => s.Id == dto.SerieId && s.Activo)
            .Select(s => s.Titulo)
            .FirstOrDefaultAsync();

        if (serieTitulo == null)
        {
            return BadRequest(new { mensaje = $"La serie con ID {dto.SerieId} no existe o está inactiva." });
        }

        var temporada = new Temporada
        {
            Numero = dto.Numero,
            Titulo = titulo,
            AnioEstreno = dto.AnioEstreno,
            CantidadEpisodios = dto.CantidadEpisodios,
            SerieId = dto.SerieId,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        _context.Temporadas.Add(temporada);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = temporada.Id }, ToDto(temporada, serieTitulo));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TemporadaUpdateDto dto)
    {
        var existing = await _context.Temporadas.FindAsync(id);
        if (existing == null || !existing.Activo)
        {
            return NotFound(new { mensaje = $"Temporada con ID {id} no existe o no se encuentra activa." });
        }

        var titulo = dto.Titulo.Trim();

        if (string.IsNullOrWhiteSpace(titulo))
        {
            return BadRequest(new { mensaje = "El título de la temporada es obligatorio." });
        }

        var existeSerie = await _context.Series
            .AnyAsync(s => s.Id == dto.SerieId && s.Activo);
        if (!existeSerie)
        {
            return BadRequest(new { mensaje = $"La serie con ID {dto.SerieId} no existe o está inactiva." });
        }

        existing.Numero = dto.Numero;
        existing.Titulo = titulo;
        existing.AnioEstreno = dto.AnioEstreno;
        existing.CantidadEpisodios = dto.CantidadEpisodios;
        existing.SerieId = dto.SerieId;
        existing.Activo = dto.Activo;
        if (!dto.Activo && existing.EliminadoEn == null)
        {
            existing.EliminadoEn = DateTime.UtcNow;
        }
        else if (dto.Activo)
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
        if (temporada == null || !temporada.Activo)
        {
            return NotFound(new { mensaje = $"Temporada con ID {id} no existe o ya no se encuentra activa." });
        }

        temporada.Activo = false;
        temporada.EliminadoEn = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static TemporadaResponseDto ToDto(Temporada temporada, string? serieTitulo = null) => new()
    {
        Id = temporada.Id,
        Numero = temporada.Numero,
        Titulo = temporada.Titulo,
        AnioEstreno = temporada.AnioEstreno,
        CantidadEpisodios = temporada.CantidadEpisodios,
        SerieId = temporada.SerieId,
        SerieTitulo = serieTitulo,
        Activo = temporada.Activo,
        CreadoEn = temporada.CreadoEn,
        EliminadoEn = temporada.EliminadoEn
    };
}
