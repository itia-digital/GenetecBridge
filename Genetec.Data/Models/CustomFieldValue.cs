using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genetec.Data.Models;

[Table("CustomFieldValue")]
public record CustomFieldValue
{
    [Key] public Guid Guid { get; set; }

    [Column("CF30fd60cbf46340be8a4e8076dcdae701")]
    [StringLength(500)]
    public string? UIUpId { get; set; }

    [Column("CFabe5f7d18ca0444db8477291c3ab7bdd")]
    [StringLength(500)]
    public string? Campus { get; set; }

    [Column("CF52978bc6661f44dc843fe4b4bdef1ba6")]
    [StringLength(500)]
    public string? PuestoCarreraOEspecialidad { get; set; }
    
    [StringLength(10)] public string? UpId { get; set; }

}