using CatalogoApi.Data;
using CatalogoApi.Dtos;
using CatalogoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/peliculas")]
[Authorize]
public class PeliculasController : ControllerBase
{
    private readonly AppDbContext _context;

    public PeliculasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PeliculaResponseDto>>> GetAll()
    {
        var peliculas = await _context.Peliculas
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Titulo)
            .Select(p => new PeliculaResponseDto
            {
                Id = p.Id,
                Titulo = p.Titulo,
                Sinopsis = p.Sinopsis,
                AnioEstreno = p.AnioEstreno,
                DuracionMin = p.DuracionMin,
                Clasificacion = p.Clasificacion,
                PosterUrl = p.PosterUrl,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria != null && p.Categoria.Activo ? p.Categoria.Nombre : null,
                Activo = p.Activo,
                CreadoEn = p.CreadoEn,
                EliminadoEn = p.EliminadoEn
            })
            .ToListAsync();

        return Ok(peliculas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PeliculaResponseDto>> GetById(int id)
    {
        var pelicula = await _context.Peliculas
            .AsNoTracking()
            .Where(p => p.Id == id && p.Activo)
            .Select(p => new PeliculaResponseDto
            {
                Id = p.Id,
                Titulo = p.Titulo,
                Sinopsis = p.Sinopsis,
                AnioEstreno = p.AnioEstreno,
                DuracionMin = p.DuracionMin,
                Clasificacion = p.Clasificacion,
                PosterUrl = p.PosterUrl,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria != null && p.Categoria.Activo ? p.Categoria.Nombre : null,
                Activo = p.Activo,
                CreadoEn = p.CreadoEn,
                EliminadoEn = p.EliminadoEn
            })
            .FirstOrDefaultAsync();

        if (pelicula == null)
        {
            return NotFound(new { mensaje = $"Película con ID {id} no encontrada." });
        }

        return Ok(pelicula);
    }

    [HttpPost]
    public async Task<ActionResult<PeliculaResponseDto>> Create([FromBody] PeliculaCreateDto dto)
    {
        var titulo = dto.Titulo.Trim();

        if (string.IsNullOrWhiteSpace(titulo))
        {
            return BadRequest(new { mensaje = "El título de la película es obligatorio." });
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

        var pelicula = new Pelicula
        {
            Titulo = titulo,
            Sinopsis = Normalizar(dto.Sinopsis),
            AnioEstreno = dto.AnioEstreno,
            DuracionMin = dto.DuracionMin,
            Clasificacion = Normalizar(dto.Clasificacion),
            PosterUrl = Normalizar(dto.PosterUrl),
            CategoriaId = dto.CategoriaId,
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        _context.Peliculas.Add(pelicula);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = pelicula.Id }, ToDto(pelicula, categoriaNombre));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PeliculaUpdateDto dto)
    {
        var existing = await _context.Peliculas.FindAsync(id);
        if (existing == null || !existing.Activo)
        {
            return NotFound(new { mensaje = $"Película con ID {id} no existe o no se encuentra activa." });
        }

        var titulo = dto.Titulo.Trim();

        if (string.IsNullOrWhiteSpace(titulo))
        {
            return BadRequest(new { mensaje = "El título de la película es obligatorio." });
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
        existing.AnioEstreno = dto.AnioEstreno;
        existing.DuracionMin = dto.DuracionMin;
        existing.Clasificacion = Normalizar(dto.Clasificacion);
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
        var pelicula = await _context.Peliculas.FindAsync(id);
        if (pelicula == null || !pelicula.Activo)
        {
            return NotFound(new { mensaje = $"Película con ID {id} no existe o ya no se encuentra activa." });
        }

        pelicula.Activo = false;
        pelicula.EliminadoEn = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static string? Normalizar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static PeliculaResponseDto ToDto(Pelicula pelicula, string? categoriaNombre = null) => new()
    {
        Id = pelicula.Id,
        Titulo = pelicula.Titulo,
        Sinopsis = pelicula.Sinopsis,
        AnioEstreno = pelicula.AnioEstreno,
        DuracionMin = pelicula.DuracionMin,
        Clasificacion = pelicula.Clasificacion,
        PosterUrl = pelicula.PosterUrl,
        CategoriaId = pelicula.CategoriaId,
        CategoriaNombre = categoriaNombre,
        Activo = pelicula.Activo,
        CreadoEn = pelicula.CreadoEn,
        EliminadoEn = pelicula.EliminadoEn
    };
}
