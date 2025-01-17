using System.Linq.Expressions;
using Core.Data;
using Core.Data.Extensions;
using Genetec.Data.Context;
using Genetec.Data.Mappers;
using Genetec.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Genetec.Data;

public class SyncService(GenetecDbContext context)
{
    private readonly EntityMapper _entityMapper = new();

    public async Task SyncAsync(List<UpRecordValue> upItems,
        CancellationToken cancellationToken)
    {
        IEnumerable<UpRecordValue> records = upItems
            .RemoveDuplicated(e => e.Id);

        var date = DateTime.UtcNow;

        List<Entity> entities = [];
        List<Cardholder> cardHolders = [];
        List<CustomFieldValue> customFieldValues = [];
        List<AlusaControl> alusaControls = [];

        foreach (UpRecordValue i in records)
        {
            var entity = _entityMapper.Map(i);
            entities.Add(entity);

            var cardHolder = new CardHolderMapper(entity.Guid).Map(i);
            cardHolders.Add(cardHolder);

            var custom = new CustomFieldValue
            {
                Guid = entity.Guid,
                Cf30fd60cbf46340be8a4e8076dcdae701 = i.Id,
                Cfabe5f7d18ca0444db8477291c3ab7bdd = i.Campus
            };
            customFieldValues.Add(custom);

            var control = new AlusaControl
            {
                EntityGuid = Guid.NewGuid(),
                UpdatedAt = date,
                UpId = i.Id
            };
            alusaControls.Add(control);
        }

        // --- U p s e r t
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

        await RunAsync(customFieldValues,
            i => new { i.Cf30fd60cbf46340be8a4e8076dcdae701 },
            (_, value) => new CustomFieldValue
            {
                Cfabe5f7d18ca0444db8477291c3ab7bdd =
                    value.Cfabe5f7d18ca0444db8477291c3ab7bdd
            },
            cancellationToken);

        await RunAsync(alusaControls,
            i => new { i.UpId },
            (_, value) => new AlusaControl
            {
                UpdatedAt = value.UpdatedAt
            },
            cancellationToken);

        // Unset the membership: applies e.g. active professor to inactive professor group
        List<string> upIds = entities.Select(e => e.Name).ToList();
        List<Guid> guids = await context.Entities
            .Where(entity => EF.Constant(upIds).Contains(entity.Name))
            .Select(entity => entity.Guid)
            .ToListAsync(cancellationToken);
        List<CardholderMembership> memberships = guids
            .Select(guid => new CardholderMembership
            {
                GuidGroup = upItems.First().GenetecGroup,
                GuidMember = guid
            })
            .ToList();

        /* *** If this runs will kill custom membership assigned out of the syncing process ***
        await context.CardholderMemberships
            .Where(entity => EF.Constant(memberships
                    .Select(m => m.GuidMember))
                .Contains(entity.GuidMember))
            .ExecuteDeleteAsync(cancellationToken);
        */

        await RunAsync(memberships,
            i => new { i.GuidMember, i.GuidGroup },
            (_, value) => new CardholderMembership
            {
                GuidGroup = value.GuidGroup,
                GuidMember = value.GuidMember
            },
            cancellationToken);
    }

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        const string sql = """
                           DELETE
                                FROM CardholderMembership
                                WHERE GuidMember IN (SELECT Guid FROM Entity INNER JOIN AlusaControl ON Entity.Name = UpId);
                           DELETE FROM CustomFieldValue WHERE Guid IN (SELECT EntityGuid FROM AlusaControl);
                           DELETE FROM Cardholder WHERE Guid IN (SELECT EntityGuid FROM AlusaControl);
                           DELETE FROM Entity WHERE Guid IN (SELECT EntityGuid FROM AlusaControl);
                           DELETE FROM AlusaControl WHERE UpdatedAt IS NOT NULL;
                           """;

        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public async Task<int> CountSyncedAsync(CancellationToken cancellationToken = default)
    {
        return await context.AlusaControls.CountAsync(cancellationToken);
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