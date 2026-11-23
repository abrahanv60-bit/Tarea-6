using CatalogoApi.Data;
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
    public async Task<IActionResult> GetAll()
    {
        var peliculas = await _context.Peliculas
            .Where(p => p.Activo)
            .ToListAsync();
        return Ok(peliculas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pelicula = await _context.Peliculas
            .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

        if (pelicula == null)
        {
            return NotFound(new { mensaje = $"Película con ID {id} no encontrada." });
        }
        return Ok(pelicula);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Pelicula pelicula)
    {
        if (string.IsNullOrWhiteSpace(pelicula.Titulo))
        {
            return BadRequest(new { mensaje = "El título de la película es obligatorio." });
        }

        if (pelicula.CategoriaId.HasValue)
        {
            var existeCategoria = await _context.Categorias
                .AnyAsync(c => c.Id == pelicula.CategoriaId.Value && c.Activo);
            if (!existeCategoria)
            {
                return BadRequest(new { mensaje = $"La categoría con ID {pelicula.CategoriaId.Value} no existe o está inactiva." });
            }
        }

        pelicula.CreadoEn = DateTime.UtcNow;
        _context.Peliculas.Add(pelicula);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = pelicula.Id }, pelicula);
    }

   [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Pelicula pelicula)
    {
        if (id != pelicula.Id)
        {
            return BadRequest(new { mensaje = "El ID de la ruta no coincide con el ID del cuerpo." });
        }

        if (string.IsNullOrWhiteSpace(pelicula.Titulo))
        {
            return BadRequest(new { mensaje = "El título de la película es obligatorio." });
        }

        var existing = await _context.Peliculas.FindAsync(id);
        if (existing == null || !existing.Activo)
        {
            return NotFound(new { mensaje = $"Película con ID {id} no existe o no se encuentra activa." });
        }

        if (pelicula.CategoriaId.HasValue)
        {
            var existeCategoria = await _context.Categorias
                .AnyAsync(c => c.Id == pelicula.CategoriaId.Value && c.Activo);
            if (!existeCategoria)
            {
                return BadRequest(new { mensaje = $"La categoría con ID {pelicula.CategoriaId.Value} no existe o está inactiva." });
            }
        }

        existing.Titulo = pelicula.Titulo;
        existing.Sinopsis = pelicula.Sinopsis;
        existing.AnioEstreno = pelicula.AnioEstreno;
        existing.DuracionMin = pelicula.DuracionMin;
        existing.Clasificacion = pelicula.Clasificacion;
        existing.PosterUrl = pelicula.PosterUrl;
        existing.CategoriaId = pelicula.CategoriaId;
        existing.Activo = pelicula.Activo;
        if (!pelicula.Activo && existing.EliminadoEn == null)
        {
            existing.EliminadoEn = pelicula.EliminadoEn ?? DateTime.UtcNow;
        }
        else if (pelicula.Activo)
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
}