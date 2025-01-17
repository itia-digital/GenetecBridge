using Core.Data;
using Genetec.Data.Mappers;
using Genetec.Data.Models;
using UP.Data;

namespace Genetec.Data;

public class ActiveEmployeesSyncService(SyncService syncService, IUpUnitOfWork unitOfWork)
{
    private readonly EntityMapper _entityMapper = new();

    public async Task SyncAsync()
    {
        // Entities
        await foreach (List<UpRecordValue> upItems in unitOfWork.ActiveEmployees
                           .FetchAllRecordsInChunksAsync())
        {
            List<Entity> entities = [];
            List<Cardholder> cardHolders = [];
            List<CardholderMembership> memberships = [];

            foreach (UpRecordValue i in upItems)
            {
                var entity = _entityMapper.Map(i);
                entities.Add(entity);

                var cardHolder = new CardHolderMapper(entity.Guid).Map(i);
                cardHolders.Add(cardHolder);

                var membership = new CardholderMembership
                {
                    GuidGroup = i.GenetecGroup,
                    GuidMember = entity.Guid,
                };
                memberships.Add(membership);
            }

            await syncService.RunAsync(entities,
                i => new { i.Name },
                (_, _) => new Entity
                {
                    Type = Constants.GenetecDefaultEntityType,
                    Version = Constants.GenetecDefaultEntityVerion,
                });

            await syncService.RunAsync(cardHolders,
                i => new { i.FirstName },
                (_, value) => new Cardholder
                {
                    Email = value.Email,
                    LastName = value.LastName,
                    MobilePhoneNumber = value.MobilePhoneNumber,
                });

            await syncService.RunAsync(memberships,
                i => new { i.GuidMember, i.GuidGroup },
                (_, value) => new CardholderMembership
                {
                    GuidGroup = value.GuidGroup,
                    GuidMember = value.GuidMember
                });
        }
    }
}