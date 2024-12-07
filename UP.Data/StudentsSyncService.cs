using Core;
using Genetec.Services;
using UP.Services.Entities;
using UP.Services.Repositories;

namespace UP.Services;

public class StudentsSyncService(IUserService userService, IUnitOfWork unitOfWork)
{
    public async Task RunAsync(CursorQueryParams @params)
    {
        CursorPagedRecords<PsUpIdGralVw> data = await unitOfWork.Students.GetPaginatedAsync(@params);
        userService.GetUserAsync()
    }
    
}