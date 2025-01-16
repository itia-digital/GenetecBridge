using UP.Data.Context;
using UP.Data.Repositories;

namespace UP.Data;

// ReSharper disable once InconsistentNaming
public interface IUpUnitOfWork : IDisposable, IAsyncDisposable
{
    IActiveEmployeesRepository Graduates { get; }
    //IStudentsRepository Students { get; }
}

public class UpUnitOfWork(UpDbContext context) : IUpUnitOfWork
{
    public void Dispose()
    {
        context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await context.DisposeAsync();
    }

    private IActiveEmployeesRepository? _graduates;

    public IActiveEmployeesRepository Graduates =>
        _graduates ??= new ActiveEmployeesRepository(context);
}
