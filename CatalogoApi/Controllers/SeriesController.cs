using CatalogoApi.Data;
using CatalogoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/series")]
[Authorize]
public class SeriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public SeriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var series = await _context.Series
            .Where(s => s.Activo)
            .ToListAsync();
        return Ok(series);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var serie = await _context.Series
            .FirstOrDefaultAsync(s => s.Id == id && s.Activo);

        if (serie == null)
        {
            return NotFound(new { mensaje = $"Serie con ID {id} no encontrada." });
        }
        return Ok(serie);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Serie serie)
    {
        if (string.IsNullOrWhiteSpace(serie.Titulo))
        {
            return BadRequest(new { mensaje = "El título de la serie es obligatorio." });
        }

        if (serie.CategoriaId.HasValue)
        {
            var existeCategoria = await _context.Categorias
                .AnyAsync(c => c.Id == serie.CategoriaId.Value && c.Activo);
            if (!existeCategoria)
            {
                return BadRequest(new { mensaje = $"La categoría con ID {serie.CategoriaId.Value} no existe o está inactiva." });
            }
        }

        serie.CreadoEn = DateTime.UtcNow;
        _context.Series.Add(serie);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = serie.Id }, serie);
    }

   [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Serie serie)
    {
        if (id != serie.Id)
        {
            return BadRequest(new { mensaje = "El ID de la ruta no coincide con el ID del cuerpo." });
        }

        if (string.IsNullOrWhiteSpace(serie.Titulo))
        {
            return BadRequest(new { mensaje = "El título de la serie es obligatorio." });
        }

        var existing = await _context.Series.FindAsync(id);
        if (existing == null || !existing.Activo)
        {
            return NotFound(new { mensaje = $"Serie con ID {id} no existe o no se encuentra activa." });
        }

        if (serie.CategoriaId.HasValue)
        {
            var existeCategoria = await _context.Categorias
                .AnyAsync(c => c.Id == serie.CategoriaId.Value && c.Activo);
            if (!existeCategoria)
            {
                return BadRequest(new { mensaje = $"La categoría con ID {serie.CategoriaId.Value} no existe o está inactiva." });
            }
        }

        existing.Titulo = serie.Titulo;
        existing.Sinopsis = serie.Sinopsis;
        existing.AnioInicio = serie.AnioInicio;
        existing.EnEmision = serie.EnEmision;
        existing.PosterUrl = serie.PosterUrl;
        existing.CategoriaId = serie.CategoriaId;
        existing.Activo = serie.Activo;
        if (!serie.Activo && existing.EliminadoEn == null)
        {
            existing.EliminadoEn = serie.EliminadoEn ?? DateTime.UtcNow;
        }
        else if (serie.Activo)
        {
            existing.EliminadoEn = null;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var serie = await _context.Series.FindAsync(id);
        if (serie == null || !serie.Activo)
        {
            return NotFound(new { mensaje = $"Serie con ID {id} no existe o ya no se encuentra activa." });
        }

        serie.Activo = false;
        serie.EliminadoEn = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}