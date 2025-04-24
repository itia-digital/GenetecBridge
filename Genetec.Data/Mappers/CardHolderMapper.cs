using Core.Data;
using Genetec.Data.Models;

namespace Genetec.Data.Mappers;

public class CardHolderMapper(Guid entityId) : IMapper<UpRecordValue, Cardholder>
{
    public Cardholder Map(UpRecordValue source)
    {
        return new Cardholder
        {
            Guid = entityId,
            FirstName = source.Name,
            LastName = source.LastName,
            Status = source.IsActive
                ? (byte)0
                : (byte)1,
            ExpirationMode = 0,
            ExpirationDuration = 0,
            ExpirationDate = null,
            ActivationDate = DateTime.UtcNow,
            Email = source.Email,
            AntipassbackExemption = false,
            ExtendedGrantTime = false,
            Info = null,
            Escort = null,
            Escort2 = null,
            MandatoryEscort = false,
            CanEscort = false,
            VisitDate = null,
            MobilePhoneNumber = source.Phone ?? string.Empty,
            Escort2Navigation = null,
            EscortNavigation = null,
            UpId = source.Id
        };
    }
}