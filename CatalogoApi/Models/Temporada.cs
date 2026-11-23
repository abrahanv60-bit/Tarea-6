using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CatalogoApi.Models;

[Table("temporadas")]
public class Temporada
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("numero")]
    public int Numero { get; set; }

    [Required]
    [Column("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [Column("anio_estreno")]
    public int AnioEstreno { get; set; }

    [Column("cantidad_episodios")]
    public int CantidadEpisodios { get; set; }

    [Column("serie_id")]
    public int SerieId { get; set; }

    [JsonIgnore]
    public Serie Serie { get; set; } = null!;

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("creado_en")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    [Column("eliminado_en")]
    public DateTime? EliminadoEn { get; set; }
}