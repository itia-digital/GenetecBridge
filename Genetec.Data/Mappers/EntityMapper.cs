using Core.Data;
using Genetec.Data.Models;
using UP.Data.Repositories;

namespace Genetec.Data.Mappers;

public class EntityMapper : IMapper<UpRecordValue, Entity>
{
    public Entity Map(UpRecordValue source)
    {
        return new Entity
        {
            Name = source.Id,
            Guid = Guid.NewGuid(),
            Type = Constants.GenetecDefaultEntityType,
            Version = Constants.GenetecDefaultEntityVerion,
            CreationTime = DateTime.UtcNow,
            Description = string.Empty,
            SubType = 0,
            Flags = 0
        };
    }
}