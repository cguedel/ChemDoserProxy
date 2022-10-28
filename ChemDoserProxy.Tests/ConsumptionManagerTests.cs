using System.Text.Json;
using ChemDoserProxy.Configuration;
using ChemDoserProxy.Dto;
using ChemDoserProxy.State;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ChemDoserProxy.Tests;

public class ConsumptionManagerTests
{
    [Fact]
    public async Task Should_read_state_from_existing_file_on_first_increment()
    {
        var (manager, settings) = Setup();
        await manager.IncrementAmountConsumed(ChemicalType.ChlorPure, 50);

        manager.State.ChlorPure.Should().Be(-50);

        var fileInfo = new FileInfo(settings.StateFile);
        fileInfo.Exists.Should().BeTrue();

        await using var fs = fileInfo.OpenRead();
        var fileContents = await JsonSerializer.DeserializeAsync<ChemicalsLevels>(fs);
        fileContents.Should().NotBeNull();
        fileContents!.ChlorPure.Should().Be(-50);
    }

    [Fact]
    public async Task Should_reset_to_capacity()
    {
        var (manager, settings) = Setup();
        await manager.RefillChemical(ChemicalType.ChlorPure);

        manager.State.ChlorPure.Should().Be(settings.ChlorPureCapacity);

        var fileInfo = new FileInfo(settings.StateFile);
        fileInfo.Exists.Should().BeTrue();

        await using var fs = fileInfo.OpenRead();
        var fileContents = await JsonSerializer.DeserializeAsync<ChemicalsLevels>(fs);
        fileContents.Should().NotBeNull();
        fileContents!.ChlorPure.Should().Be(settings.ChlorPureCapacity);
    }

    private static (ChemicalsManager manager, ChemicalsSettings settings) Setup()
    {
        var settings = new Mock<IOptions<ChemicalsSettings>>();
        settings.SetupGet(m => m.Value).Returns(new ChemicalsSettings
        {
            StateFile = $"{Guid.NewGuid():N}.json",
            ChlorPureCapacity = 100
        });

        var manager = new ChemicalsManager(settings.Object);
        return (manager, settings.Object.Value);
    }
}
