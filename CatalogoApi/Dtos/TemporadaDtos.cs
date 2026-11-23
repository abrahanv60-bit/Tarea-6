using System.ComponentModel.DataAnnotations;

namespace CatalogoApi.Dtos;

public class TemporadaResponseDto
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int AnioEstreno { get; set; }
    public int CantidadEpisodios { get; set; }
    public int SerieId { get; set; }
    public string? SerieTitulo { get; set; }
    public bool Activo { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime? EliminadoEn { get; set; }
}

public class TemporadaCreateDto
{
    [Range(1, 100, ErrorMessage = "El número de temporada debe estar entre 1 y 100.")]
    public int Numero { get; set; }

    [Required(ErrorMessage = "El título de la temporada es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Range(1888, 2100, ErrorMessage = "El año de estreno debe estar entre 1888 y 2100.")]
    public int AnioEstreno { get; set; }

    [Range(0, 1000, ErrorMessage = "La cantidad de episodios debe estar entre 0 y 1000.")]
    public int CantidadEpisodios { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La serie debe ser un ID válido.")]
    public int SerieId { get; set; }
}

public class TemporadaUpdateDto
{
    [Range(1, 100, ErrorMessage = "El número de temporada debe estar entre 1 y 100.")]
    public int Numero { get; set; }

    [Required(ErrorMessage = "El título de la temporada es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Range(1888, 2100, ErrorMessage = "El año de estreno debe estar entre 1888 y 2100.")]
    public int AnioEstreno { get; set; }

    [Range(0, 1000, ErrorMessage = "La cantidad de episodios debe estar entre 0 y 1000.")]
    public int CantidadEpisodios { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La serie debe ser un ID válido.")]
    public int SerieId { get; set; }

    public bool Activo { get; set; } = true;
}
