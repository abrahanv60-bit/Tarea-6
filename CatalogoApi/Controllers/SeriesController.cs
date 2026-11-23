using CatalogoApi.Data;
using CatalogoApi.Dtos;
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
    public async Task<ActionResult<IEnumerable<SerieResponseDto>>> GetAll()
    {
        var series = await _context.Series
            .AsNoTracking()
            .Where(s => s.Activo)
            .OrderBy(s => s.Titulo)
            .Select(s => new SerieResponseDto
            {
                Id = s.Id,
                Titulo = s.Titulo,
                Sinopsis = s.Sinopsis,
                AnioInicio = s.AnioInicio,
                EnEmision = s.EnEmision,
                PosterUrl = s.PosterUrl,
                CategoriaId = s.CategoriaId,
                CategoriaNombre = s.Categoria != null && s.Categoria.Activo ? s.Categoria.Nombre : null,
                Activo = s.Activo,
                CreadoEn = s.CreadoEn,
                EliminadoEn = s.EliminadoEn
            })
            .ToListAsync();

        return Ok(series);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SerieResponseDto>> GetById(int id)
    {
        var serie = await _context.Series
            .AsNoTracking()
            .Where(s => s.Id == id && s.Activo)
            .Select(s => new SerieResponseDto
            {
                Id = s.Id,
                Titulo = s.Titulo,
                Sinopsis = s.Sinopsis,
                AnioInicio = s.AnioInicio,
                EnEmision = s.EnEmision,
                PosterUrl = s.PosterUrl,
                CategoriaId = s.CategoriaId,
                CategoriaNombre = s.Categoria != null && s.Categoria.Activo ? s.Categoria.Nombre : null,
                Activo = s.Activo,
                CreadoEn = s.CreadoEn,
                EliminadoEn = s.EliminadoEn
            })
            .FirstOrDefaultAsync();

        if (serie == null)
        {
            return NotFound(new { mensaje = $"Serie con ID {id} no encontrada." });
        }

        return Ok(serie);
    }

    [HttpPost]
    public async Task<ActionResult<SerieResponseDto>> Create([FromBody] SerieCreateDto dto)
    {
        var titulo = dto.Titulo.Trim();

        if (string.IsNullOrWhiteSpace(titulo))
        {
            return BadRequest(new { mensaje = "El título de la serie es obligatorio." });
        }

        string? categoriaNombre = null;
        if (dto.CategoriaId.HasValue)
        {
            categoriaNombre = await _context.Categorias
                .Where(c => c.Id == dto.CategoriaId.Value && c.Activo)
                .Select(c => c.Nombre)
                .FirstOrDefaultAsync();

            if (categoriaNombre == null)
            {
                return BadRequest(new { mensaje = $"La categoría con ID {dto.CategoriaId.Value} no existe o está inactiva." });
            }
        }

        var serie = new Serie
        {
            Titulo = titulo,
            Sinopsis = Normalizar(dto.Sinopsis),
            AnioInicio = dto.AnioInicio,
            EnEmision = dto.EnEmision,
            PosterUrl = Normalizar(dto.PosterUrl),
            CategoriaId = dto.CategoriaId,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        _context.Series.Add(serie);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = serie.Id }, ToDto(serie, categoriaNombre));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SerieUpdateDto dto)
    {
        var existing = await _context.Series.FindAsync(id);
        if (existing == null || !existing.Activo)
        {
            return NotFound(new { mensaje = $"Serie con ID {id} no existe o no se encuentra activa." });
        }

        var titulo = dto.Titulo.Trim();

        if (string.IsNullOrWhiteSpace(titulo))
        {
            return BadRequest(new { mensaje = "El título de la serie es obligatorio." });
        }

        if (dto.CategoriaId.HasValue)
        {
            var existeCategoria = await _context.Categorias
                .AnyAsync(c => c.Id == dto.CategoriaId.Value && c.Activo);
            if (!existeCategoria)
            {
                return BadRequest(new { mensaje = $"La categoría con ID {dto.CategoriaId.Value} no existe o está inactiva." });
            }
        }

        existing.Titulo = titulo;
        existing.Sinopsis = Normalizar(dto.Sinopsis);
        existing.AnioInicio = dto.AnioInicio;
        existing.EnEmision = dto.EnEmision;
        existing.PosterUrl = Normalizar(dto.PosterUrl);
        existing.CategoriaId = dto.CategoriaId;
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

    private static string? Normalizar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static SerieResponseDto ToDto(Serie serie, string? categoriaNombre = null) => new()
    {
        Id = serie.Id,
        Titulo = serie.Titulo,
        Sinopsis = serie.Sinopsis,
        AnioInicio = serie.AnioInicio,
        EnEmision = serie.EnEmision,
        PosterUrl = serie.PosterUrl,
        CategoriaId = serie.CategoriaId,
        CategoriaNombre = categoriaNombre,
        Activo = serie.Activo,
        CreadoEn = serie.CreadoEn,
        EliminadoEn = serie.EliminadoEn
    };
}
