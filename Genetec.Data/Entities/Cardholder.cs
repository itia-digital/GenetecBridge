namespace Genetec.Data.Entities;

public class Cardholder
{
    public Guid Guid { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Guid? Picture { get; set; }
    public Guid? Thumbnail { get; set; }
    public byte Status { get; set; }
    public byte ExpirationMode { get; set; }
    public int? ExpirationDuration { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? ActivationDate { get; set; }
    public string? Email { get; set; }
    public bool AntipassbackExemption { get; set; }
    public bool ExtendedGrantTime { get; set; }
    public string? Info { get; set; }
    public Guid? Escort { get; set; }
    public Guid? Escort2 { get; set; }
    public bool MandatoryEscort { get; set; }
    public bool CanEscort { get; set; }
    public DateTime? VisitDate { get; set; }
    public string? MobilePhoneNumber { get; set; }

    public Entity? Entity { get; set; }
    public Entity? EscortEntity { get; set; }
    public Entity? Escort2Entity { get; set; }
}