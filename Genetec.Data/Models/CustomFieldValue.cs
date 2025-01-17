using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genetec.Data.Models;

[Table("CustomFieldValue")]
public partial record CustomFieldValue
{
    [Key]
    public Guid Guid { get; set; }

    [Column("CF30fd60cbf46340be8a4e8076dcdae701")]
    [StringLength(500)]
    public string? Cf30fd60cbf46340be8a4e8076dcdae701 { get; set; }

    [Column("CFabe5f7d18ca0444db8477291c3ab7bdd")]
    [StringLength(500)]
    public string? Cfabe5f7d18ca0444db8477291c3ab7bdd { get; set; }
}
