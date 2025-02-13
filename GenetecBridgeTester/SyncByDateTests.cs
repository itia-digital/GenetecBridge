﻿using Core.Data;
using Genetec.Data;

namespace GenetecBridgeTester;

public class SyncByDateTests
{
    private readonly SyncService _syncService = new(Utils.GetLogger<SyncService>());

    [Fact]
    public async Task SyncService_SyncsAllSuccessfully()
    {
        // arrange
        await _syncService.ClearAsync();
        
        // act
        await _syncService.SyncAllAsync();

        // assert
    }

    [Theory]
    [InlineData("2002-05-29")]
    public async Task SyncService_SyncsSuccessfully(
        string date)
    {
        // arrange
        // act
        await _syncService.SyncAsync(DateTime.Parse(date));

        // assert;
    }
}