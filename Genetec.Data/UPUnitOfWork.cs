using Genetec.Data.Context;

namespace Genetec.Data;

// ReSharper disable once InconsistentNaming
public interface IUPUnitOfWork : IDisposable
{
    IEntitiesRepository Graduates { get; }
}

public class UPUnitOfWork(GenetecDbContext context) : IUPUnitOfWork
{
    private IEntitiesRepository? _graduates;

    public IEntitiesRepository Graduates =>
        _graduates ??= new EntitiesRepository(context);

    public void Dispose()
    {
        context.Dispose();
    }
}