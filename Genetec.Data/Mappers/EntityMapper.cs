using Core.Data;
using Genetec.Data.Models;

namespace Genetec.Data.Mappers;

public class EntityMapper : IMapper<UpRecordValue, Entity>
{
    public Entity Map(UpRecordValue source)
    {
        return new Entity
        {
            Name = source.FullName,
            Guid = Guid.NewGuid(),
            Type = Constants.GenetecCardHolderEntityType,
            Version = Constants.GenetecDefaultEntityVerion,
            CreationTime = DateTime.UtcNow,
            Description = string.Empty,
            SubType = 0,
            Flags = 0,
            UpId = source.Id
        };
    }
}