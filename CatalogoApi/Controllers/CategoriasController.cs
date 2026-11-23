using CatalogoApi.Data;
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
    public async Task<IActionResult> GetAll()
    {
        var categorias = await _context.Categorias
            .Where(c => c.Activo)
            .ToListAsync();
        return Ok(categorias);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

        if (categoria == null)
        {
            return NotFound(new { mensaje = $"Categoría con ID {id} no encontrada." });
        }
        return Ok(categoria);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Categoria categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria.Nombre))
        {
            return BadRequest(new { mensaje = "El nombre de la categoría es obligatorio." });
        }

        var existeNombre = await _context.Categorias
            .AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.Trim().ToLower());
        if (existeNombre)
        {
            return Conflict(new { mensaje = $"Ya existe una categoría con el nombre '{categoria.Nombre}'." });
        }

        categoria.CreadoEn = DateTime.UtcNow;
        _context.Categorias.Add(categoria);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.Trim().ToLower()))
            {
                return Conflict(new { mensaje = $"Ya existe una categoría con el nombre '{categoria.Nombre}'." });
            }
            throw;
        }

        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Categoria categoria)
    {
        if (id != categoria.Id)
        {
            return BadRequest(new { mensaje = "El ID de la ruta no coincide con el ID del cuerpo." });
        }

        if (string.IsNullOrWhiteSpace(categoria.Nombre))
        {
            return BadRequest(new { mensaje = "El nombre de la categoría es obligatorio." });
        }

        var existing = await _context.Categorias.FindAsync(id);
        if (existing == null || !existing.Activo)
        {
            return NotFound(new { mensaje = $"Categoría con ID {id} no existe o no se encuentra activa." });
        }

        var existeNombre = await _context.Categorias
            .AnyAsync(c => c.Id != id && c.Nombre.ToLower() == categoria.Nombre.Trim().ToLower());
        if (existeNombre)
        {
            return Conflict(new { mensaje = $"Ya existe otra categoría con el nombre '{categoria.Nombre}'." });
        }

        existing.Nombre = categoria.Nombre;
        existing.Descripcion = categoria.Descripcion;
        existing.Activo = categoria.Activo;
        if (!categoria.Activo && existing.EliminadoEn == null)
        {
            existing.EliminadoEn = categoria.EliminadoEn ?? DateTime.UtcNow;
        }
        else if (categoria.Activo)
        {
            existing.EliminadoEn = null;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (await _context.Categorias.AnyAsync(c => c.Id != id && c.Nombre.ToLower() == categoria.Nombre.Trim().ToLower()))
            {
                return Conflict(new { mensaje = $"Ya existe otra categoría con el nombre '{categoria.Nombre}'." });
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
}