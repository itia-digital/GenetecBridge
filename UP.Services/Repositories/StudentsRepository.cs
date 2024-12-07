using Core;
using UP.Services.Context;
using UP.Services.Entities;

namespace UP.Services.Repositories;

public class StudentsRepository(UpDbContext context)
    : CursorPaginationRepository<PsUpIdGralVw, UpDbContext>(context)
{
    protected override IQueryable<PsUpIdGralVw> BaseQuery() 
        => base.BaseQuery().Where(e => e.UpCountProg > 0);
}