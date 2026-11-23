using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CatalogoApi.Models;

[Table("categorias")]
public class Categoria
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("creado_en")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    [Column("eliminado_en")]
    public DateTime? EliminadoEn { get; set; }

    [JsonIgnore]
    public ICollection<Pelicula> Peliculas { get; set; } = new List<Pelicula>();

    [JsonIgnore]
    public ICollection<Serie> Series { get; set; } = new List<Serie>();
}