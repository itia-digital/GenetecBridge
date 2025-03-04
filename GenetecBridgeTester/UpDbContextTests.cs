using UP.Data.Context;
using UP.Data.Repositories;

namespace GenetecBridgeTester;

public class UpDbContextTests
{
    private readonly IUtilitiesRepository _utilitiesRepository = new UtilitiesRepository(new UpDbContext());
    
    [Fact]
    public async Task GetActiveRecordsAsync_FetchesAllActiveRecords()
    {
        var result = await _utilitiesRepository.GetActiveRecordsAsync();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
    
    [Theory]
    [InlineData("2023-02-03")]
    [InlineData("2019-04-08")]
    [InlineData("2025-02-28")]
    public async Task GetActiveRecordsAsync_FetchesAllActiveRecordsByDate(string date)
    {
        var d = DateTime.Parse(date);
        var result = await _utilitiesRepository.GetActiveRecordsAsync(d);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}