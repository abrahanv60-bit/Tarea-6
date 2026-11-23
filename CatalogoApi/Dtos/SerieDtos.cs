using System.ComponentModel.DataAnnotations;

namespace CatalogoApi.Dtos;

public class SerieResponseDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Sinopsis { get; set; }
    public int AnioInicio { get; set; }
    public bool EnEmision { get; set; }
    public string? PosterUrl { get; set; }
    public int? CategoriaId { get; set; }
    public string? CategoriaNombre { get; set; }
    public bool Activo { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime? EliminadoEn { get; set; }
}

public class SerieCreateDto
{
    [Required(ErrorMessage = "El título de la serie es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "La sinopsis no puede superar los 2000 caracteres.")]
    public string? Sinopsis { get; set; }

    [Range(1900, 2100, ErrorMessage = "El año de inicio debe estar entre 1900 y 2100.")]
    public int AnioInicio { get; set; }

    public bool EnEmision { get; set; }

    [StringLength(500, ErrorMessage = "La URL del póster no puede superar los 500 caracteres.")]
    public string? PosterUrl { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La categoría debe ser un ID válido.")]
    public int? CategoriaId { get; set; }
}

public class SerieUpdateDto
{
    [Required(ErrorMessage = "El título de la serie es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "La sinopsis no puede superar los 2000 caracteres.")]
    public string? Sinopsis { get; set; }

    [Range(1900, 2100, ErrorMessage = "El año de inicio debe estar entre 1900 y 2100.")]
    public int AnioInicio { get; set; }

    public bool EnEmision { get; set; }

    [StringLength(500, ErrorMessage = "La URL del póster no puede superar los 500 caracteres.")]
    public string? PosterUrl { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La categoría debe ser un ID válido.")]
    public int? CategoriaId { get; set; }

    public bool Activo { get; set; } = true;
}
