using System.Linq.Expressions;
using Core.Data;
using Genetec.Data.Context;
using Genetec.Data.Mappers;
using Genetec.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Genetec.Data;

public class SyncService(GenetecDbContext context)
{
    private readonly EntityMapper _entityMapper = new();
    
    public async Task SyncAsync(List<UpRecordValue> upItems, CancellationToken cancellationToken)
    {
            List<Entity> entities = [];
            List<Cardholder> cardHolders = [];
            List<CardholderMembership> memberships = [];
            List<CustomFieldValue> customFieldValues = [];

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

                var custom = new CustomFieldValue
                {
                    Guid = entity.Guid,
                    Cf30fd60cbf46340be8a4e8076dcdae701 = i.Id,
                    Cfabe5f7d18ca0444db8477291c3ab7bdd = i.Campus
                };
                customFieldValues.Add(custom);
            }

            await RunAsync(entities,
                i => new { i.Name },
                (_, _) => new Entity
                {
                    Type = Constants.GenetecDefaultEntityType,
                    Version = Constants.GenetecDefaultEntityVerion,
                },
                cancellationToken);

            await RunAsync(cardHolders,
                i => new { i.FirstName },
                (_, value) => new Cardholder
                {
                    Email = value.Email,
                    LastName = value.LastName,
                    MobilePhoneNumber = value.MobilePhoneNumber,
                },
                cancellationToken);

            // Unset the membership: applies e.g. active professor to inactive professor group
            List<Guid> guids = entities.Select(e => e.Guid).ToList();
            await context.CardholderMemberships
                .Where(c => guids.Contains(c.GuidMember))
                .ExecuteDeleteAsync(cancellationToken);

            await RunAsync(memberships,
                i => new { i.GuidMember, i.GuidGroup },
                (_, value) => new CardholderMembership
                {
                    GuidGroup = value.GuidGroup,
                    GuidMember = value.GuidMember
                },
                cancellationToken);

            await RunAsync(customFieldValues,
                i => new { i.Cf30fd60cbf46340be8a4e8076dcdae701 },
                (_, value) => new CustomFieldValue
                {
                    Cfabe5f7d18ca0444db8477291c3ab7bdd =
                        value.Cfabe5f7d18ca0444db8477291c3ab7bdd
                },
                cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data">Chunk of data to sync</param>
    /// <param name="matching">Expression to match or create</param>
    /// <param name="whenMatched">Expression to update values when found (existingValue, newValue) => finaleValue</param>
    /// <param name="cancellationToken"></param>
    private async Task RunAsync<TGenetec>(
        List<TGenetec> data,
        Expression<Func<TGenetec, object>> matching,
        Expression<Func<TGenetec, TGenetec, TGenetec>> whenMatched,
        CancellationToken cancellationToken)
        where TGenetec : class
    {
        await context.Set<TGenetec>()
            .UpsertRange(data)
            .On(matching)
            .WhenMatched(whenMatched)
            .RunAsync(cancellationToken);
    }
}