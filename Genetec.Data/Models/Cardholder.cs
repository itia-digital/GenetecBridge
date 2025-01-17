using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Genetec.Data.Models;

[Table("Cardholder")]
[Index("ExpirationMode", Name = "IX_CardholderExpirationMode")]
[Index("FirstName", Name = "IX_CardholderFirstName")]
[Index("LastName", Name = "IX_CardholderLastName")]
[Index("Picture", Name = "IX_CardholderPicture")]
[Index("Thumbnail", Name = "IX_CardholderThumbnail")]
[Index("ExpirationMode", Name = "IX_CredentialExpirationMode")]
[Index("Escort", Name = "IX_Escort")]
[Index("Escort2", Name = "IX_Escort2")]
public partial record Cardholder
{
    [Key]
    public Guid Guid { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    public Guid? Picture { get; set; }

    public Guid? Thumbnail { get; set; }

    public byte Status { get; set; }

    public byte ExpirationMode { get; set; }

    public int? ExpirationDuration { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExpirationDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ActivationDate { get; set; }

    public string? Email { get; set; }

    public bool AntipassbackExemption { get; set; }

    public bool ExtendedGrantTime { get; set; }

    public string? Info { get; set; }

    public Guid? Escort { get; set; }

    public Guid? Escort2 { get; set; }

    public bool MandatoryEscort { get; set; }

    public bool CanEscort { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? VisitDate { get; set; }

    [StringLength(100)]
    public string? MobilePhoneNumber { get; set; }

    [ForeignKey("Escort2")]
    [InverseProperty("CardholderEscort2Navigations")]
    public virtual Entity? Escort2Navigation { get; set; }

    [ForeignKey("Escort")]
    [InverseProperty("CardholderEscortNavigations")]
    public virtual Entity? EscortNavigation { get; set; }

    [ForeignKey("Guid")]
    [InverseProperty("CardholderGu")]
    public virtual Entity Gu { get; set; } = null!;
}
