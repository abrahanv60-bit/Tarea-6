using System.ComponentModel.DataAnnotations;

namespace CatalogoApi.Dtos;

public class PeliculaResponseDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Sinopsis { get; set; }
    public int AnioEstreno { get; set; }
    public int DuracionMin { get; set; }
    public string? Clasificacion { get; set; }
    public string? PosterUrl { get; set; }
    public int? CategoriaId { get; set; }
    public string? CategoriaNombre { get; set; }
    public bool Activo { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime? EliminadoEn { get; set; }
}

public class PeliculaCreateDto
{
    [Required(ErrorMessage = "El título de la película es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "La sinopsis no puede superar los 2000 caracteres.")]
    public string? Sinopsis { get; set; }

    [Range(1888, 2100, ErrorMessage = "El año de estreno debe estar entre 1888 y 2100.")]
    public int AnioEstreno { get; set; }

    [Range(1, 1000, ErrorMessage = "La duración debe estar entre 1 y 1000 minutos.")]
    public int DuracionMin { get; set; }

    [StringLength(20, ErrorMessage = "La clasificación no puede superar los 20 caracteres.")]
    public string? Clasificacion { get; set; }

    [StringLength(500, ErrorMessage = "La URL del póster no puede superar los 500 caracteres.")]
    public string? PosterUrl { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La categoría debe ser un ID válido.")]
    public int? CategoriaId { get; set; }
}

public class PeliculaUpdateDto
{
    [Required(ErrorMessage = "El título de la película es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "La sinopsis no puede superar los 2000 caracteres.")]
    public string? Sinopsis { get; set; }

    [Range(1888, 2100, ErrorMessage = "El año de estreno debe estar entre 1888 y 2100.")]
    public int AnioEstreno { get; set; }

    [Range(1, 1000, ErrorMessage = "La duración debe estar entre 1 y 1000 minutos.")]
    public int DuracionMin { get; set; }

    [StringLength(20, ErrorMessage = "La clasificación no puede superar los 20 caracteres.")]
    public string? Clasificacion { get; set; }

    [StringLength(500, ErrorMessage = "La URL del póster no puede superar los 500 caracteres.")]
    public string? PosterUrl { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La categoría debe ser un ID válido.")]
    public int? CategoriaId { get; set; }

    public bool Activo { get; set; } = true;
}
