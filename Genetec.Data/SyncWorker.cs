﻿using Core.Data;
using Core.Data.Extensions;
using Genetec.Data.Context;
using Genetec.Data.Mappers;
using Genetec.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Genetec.Data;

public class SyncWorker(GenetecDbContext context, ILogger logger)
{
    private readonly EntityMapper _entityMapper = new();

    public async Task RunAsync(DateTime startedAt, List<UpRecordValue> records,
        CancellationToken cancellationToken)
    {
        Dictionary<string, List<UpRecordValue>> source = records.GroupBy(e => e.Id)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Entities
        List<Entity> entities = source
            .Select(g => g.Value.First())
            .Select(record => _entityMapper.Map(record))
            .Select(entity => entity with { SyncedAt = startedAt })
            .ToList();

        await context.ExecUpsertAsync(entities,
            i => $"{i.UpId}",
            i => new { i.UpId },
            (_, value) => new Entity
            {
                Type = Constants.GenetecCardHolderEntityType,
                Version = Constants.GenetecDefaultEntityVersion,
            },
            cancellationToken);

        Dictionary<string, Guid> dbEntities = await context.Entities.Where(e =>
                EF.Constant(entities.Select(e => e.UpId)).Contains(e.UpId))
            .Select(e => new { e.Guid, e.UpId })
            .ToDictionaryAsync(
                e => e.UpId!,
                e => e.Guid,
                cancellationToken: cancellationToken);

        // cardHolder
        List<Cardholder> cardHolders = source
            .Select(g => g.Value.First())
            .Select(record => new CardHolderMapper(dbEntities[record.Id]).Map(record))
            .ToList();

        await context.ExecUpsertAsync(cardHolders,
            i => $"${i.Guid}",
            i => new { i.Guid },
            (_, value) => new Cardholder
            {
                UpId = value.UpId,
                Email = value.Email,
                Status = value.Status,
                LastName = value.LastName,
                FirstName = value.FirstName,
                MobilePhoneNumber = value.MobilePhoneNumber,
            },
            cancellationToken);

        // Custom Values
        List<CustomFieldValue> customFieldValues = source
            .Select(g =>
            {
                var r = g.Value.First();
                return new CustomFieldValue
                {
                    PuestoCarreraOEspecialidad = string.Join(", ",
                        g.Value
                            .Select(e => e.PositionOrProgram)
                            .Distinct()),
                    Campus = string.Join(", ",
                        g.Value
                            .Select(e => e.Campus)
                            .Distinct()),
                    Guid = dbEntities[g.Value.First().Id],
                    UIUpId = r.Id,
                    UpId = r.Id
                };
            })
            .ToList();

        await context.ExecUpsertAsync(customFieldValues,
            i => $"{i.Guid}",
            i => new { i.Guid },
            (_, value) => new CustomFieldValue
            {
                UpId = value.UpId,
                UIUpId = value.UIUpId,
                PuestoCarreraOEspecialidad = value.PuestoCarreraOEspecialidad,
                Campus = value.Campus,
            },
            cancellationToken);


        // Membership - group
        // Unset the membership: applies e.g. active professor to inactive professor group
        List<CardholderMembership> memberships = source
            .Select(g => g.Value.First())
            .Select(record => new CardholderMembership
            {
                GuidGroup = record.GenetecGroup,
                GuidMember = dbEntities[record.Id],
            })
            .ToList();


        /* *** If this runs will kill custom membership assigned out of the syncing process ***
        await context.CardholderMemberships
            .Where(entity => EF.Constant(memberships
                    .Select(m => m.GuidMember))
                .Contains(entity.GuidMember))
            .ExecuteDeleteAsync(cancellationToken);
        */

        await context.ExecUpsertAsync(memberships,
            i => $"{i.GuidMember}-{i.GuidGroup}",
            i => new { i.GuidMember, i.GuidGroup },
            (_, value) => new CardholderMembership
            {
                GuidGroup = value.GuidGroup,
                GuidMember = value.GuidMember,
            },
            cancellationToken);

        // Membership - partitions
        // Unset the membership: applies e.g. active professor to inactive professor group
        List<PartitionMembership> partitionMemberships = source
            .Select(g => g.Value.First())
            .SelectMany(_ => new[]
                {
                    Constants.GenetecPartitionDefault,
                    Constants.GenetecPartitionMixcoac,
                    Constants.GenetecPartitionCdUp,
                    Constants.GenetecPartitionGdl,
                    Constants.GenetecPartitionAgs
                },
                (record, partitionId) => new PartitionMembership
                {
                    GuidGroup = partitionId,
                    GuidMember = dbEntities[record.Id],
                })
            .ToList();

        await context.ExecUpsertAsync(partitionMemberships,
            i => $"{i.GuidMember}-{i.GuidGroup}",
            i => new { i.GuidMember, i.GuidGroup },
            (_, value) => new PartitionMembership
            {
                GuidGroup = value.GuidGroup,
                GuidMember = value.GuidMember,
            },
            cancellationToken);

        logger.LogInformation("Synced {count} records for {date}:", records.Count, startedAt.ToString("yyyy-MM-dd"));
        foreach (var record in records)
        {
            logger.LogInformation("   {UpId} - {Name}", record.Id, record.FullName);;
        }
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
                           DELETE
                                FROM CardholderMembership
                                WHERE GuidMember IN (SELECT Guid FROM Entity WHERE UPId IS NOT NULL);
                           DELETE 
                                FROM PartitionMembership 
                                WHERE GuidMember IN (SELECT Guid FROM Entity WHERE UPId IS NOT NULL);
                           DELETE FROM CustomFieldValue 
                                  WHERE Guid IN (SELECT Guid FROM Entity WHERE UPId IS NOT NULL);
                           DELETE FROM Cardholder WHERE Guid IN (SELECT Guid FROM Entity WHERE UPId IS NOT NULL);
                           DELETE FROM Entity WHERE UPId IS NOT NULL;
                           TRUNCATE TABLE AlusaControl;
                           """;

        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public async Task CreateControlAsync(AlusaControl control,
        CancellationToken cancellationToken = default)
    {
        await context.AlusaControls.AddAsync(control, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}