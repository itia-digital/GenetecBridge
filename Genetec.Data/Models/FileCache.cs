using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genetec.Data.Models;

[Table("FileCache")]
public class FileCache
{
    [Key]
    public Guid Guid { get; set; }

    public byte[] Contents { get; set; } = null!;

    [StringLength(256)]
    public string Extension { get; set; } = null!;

    public Guid RelatedEntity { get; set; }

    public string Context { get; set; } = null!;

    [InverseProperty("PictureNavigation")]
    public virtual ICollection<Cardholder> CardholderPictureNavigations { get; set; } = new List<Cardholder>();

    [InverseProperty("ThumbnailNavigation")]
    public virtual ICollection<Cardholder> CardholderThumbnailNavigations { get; set; } = new List<Cardholder>();
}
