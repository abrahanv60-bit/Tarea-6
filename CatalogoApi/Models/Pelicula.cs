using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CatalogoApi.Models;

[Table("peliculas")]
public class Pelicula
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [Column("sinopsis")]
    public string? Sinopsis { get; set; }

    [Column("anio_estreno")]
    public int AnioEstreno { get; set; }

    [Column("duracion_min")]
    public int DuracionMin { get; set; }

    [Column("clasificacion")]
    public string? Clasificacion { get; set; }

    [Column("poster_url")]
    public string? PosterUrl { get; set; }

    [Column("categoria_id")]
    public int? CategoriaId { get; set; }

    [JsonIgnore]
    public Categoria? Categoria { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("creado_en")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    [Column("eliminado_en")]
    public DateTime? EliminadoEn { get; set; }
}