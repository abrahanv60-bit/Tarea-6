using CatalogoApi.Data;
using CatalogoApi.Dtos;
using CatalogoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/categorias")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaResponseDto>>> GetAll()
    {
        var categorias = await _context.Categorias
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaResponseDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Activo = c.Activo,
                CreadoEn = c.CreadoEn,
                EliminadoEn = c.EliminadoEn
            })
            .ToListAsync();

        return Ok(categorias);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoriaResponseDto>> GetById(int id)
    {
        var categoria = await _context.Categorias
            .AsNoTracking()
            .Where(c => c.Id == id && c.Activo)
            .Select(c => new CategoriaResponseDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Activo = c.Activo,
                CreadoEn = c.CreadoEn,
                EliminadoEn = c.EliminadoEn
            })
            .FirstOrDefaultAsync();

        if (categoria == null)
        {
            return NotFound(new { mensaje = $"Categoría con ID {id} no encontrada." });
        }

        return Ok(categoria);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaResponseDto>> Create([FromBody] CategoriaCreateDto dto)
    {
        var nombre = dto.Nombre.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new { mensaje = "El nombre de la categoría es obligatorio." });
        }

        var existeNombre = await _context.Categorias
            .AnyAsync(c => c.Nombre.ToLower() == nombre.ToLower());
        if (existeNombre)
        {
            return Conflict(new { mensaje = $"Ya existe una categoría con el nombre '{nombre}' (activa o eliminada)." });
        }

        var categoria = new Categoria
        {
            Nombre = nombre,
            Descripcion = Normalizar(dto.Descripcion),
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        _context.Categorias.Add(categoria);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == nombre.ToLower()))
            {
                return Conflict(new { mensaje = $"Ya existe una categoría con el nombre '{nombre}' (activa o eliminada)." });
            }
            throw;
        }

        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, ToDto(categoria));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoriaUpdateDto dto)
    {
        var existing = await _context.Categorias.FindAsync(id);
        if (existing == null || !existing.Activo)
        {
            return NotFound(new { mensaje = $"Categoría con ID {id} no existe o no se encuentra activa." });
        }

        var nombre = dto.Nombre.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new { mensaje = "El nombre de la categoría es obligatorio." });
        }

        var existeNombre = await _context.Categorias
            .AnyAsync(c => c.Id != id && c.Nombre.ToLower() == nombre.ToLower());
        if (existeNombre)
        {
            return Conflict(new { mensaje = $"Ya existe otra categoría con el nombre '{nombre}'." });
        }

        existing.Nombre = nombre;
        existing.Descripcion = Normalizar(dto.Descripcion);
        existing.Activo = dto.Activo;
        if (!dto.Activo && existing.EliminadoEn == null)
        {
            existing.EliminadoEn = DateTime.UtcNow;
        }
        else if (dto.Activo)
        {
            existing.EliminadoEn = null;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (await _context.Categorias.AnyAsync(c => c.Id != id && c.Nombre.ToLower() == nombre.ToLower()))
            {
                return Conflict(new { mensaje = $"Ya existe otra categoría con el nombre '{nombre}'." });
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null || !categoria.Activo)
        {
            return NotFound(new { mensaje = $"Categoría con ID {id} no existe o ya no se encuentra activa." });
        }

        categoria.Activo = false;
        categoria.EliminadoEn = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static string? Normalizar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static CategoriaResponseDto ToDto(Categoria categoria) => new()
    {
        Id = categoria.Id,
        Nombre = categoria.Nombre,
        Descripcion = categoria.Descripcion,
        Activo = categoria.Activo,
        CreadoEn = categoria.CreadoEn,
        EliminadoEn = categoria.EliminadoEn
    };
}
