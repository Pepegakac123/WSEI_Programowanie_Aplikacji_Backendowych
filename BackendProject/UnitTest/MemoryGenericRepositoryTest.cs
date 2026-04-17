using System;
using System.Linq;
using System.Threading.Tasks;
using AppCore.Models;
using AppCore.Repositories;
using Infrastructure.Repositories;
using Xunit;

namespace UnitTest;

public class MemoryGenericRepositoryTest
{
    private readonly IGenericRepositoryAsync<Vehicle> _repo = new MemoryGenericRepository<Vehicle>();

    [Fact]
    public async Task AddVehicleToRepositoryTestAsync()
    {
        // Arrange
        var expected = new Vehicle
        {
            LicensePlate = "TK 8434Y",
            Brand = "Toyota",
            Color = "Red"
        };

        // Act
        await _repo.AddAsync(expected);

        // Assert
        var actual = await _repo.FindByIdAsync(expected.Id);
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Id, actual?.Id);
    }

    [Fact]
    public async Task FindAllAsync_ShouldReturnAllItems()
    {
        // Arrange
        await _repo.AddAsync(new Vehicle { LicensePlate = "V1" });
        await _repo.AddAsync(new Vehicle { LicensePlate = "V2" });

        // Act
        var result = await _repo.FindAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task RemoveByIdAsync_ShouldRemoveItem()
    {
        // Arrange
        var vehicle = await _repo.AddAsync(new Vehicle { LicensePlate = "V1" });

        // Act
        await _repo.RemoveByIdAsync(vehicle.Id);

        // Assert
        var result = await _repo.FindByIdAsync(vehicle.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateItem()
    {
        // Arrange
        var vehicle = await _repo.AddAsync(new Vehicle { LicensePlate = "OldPlate" });
        vehicle.LicensePlate = "NewPlate";

        // Act
        await _repo.UpdateAsync(vehicle);

        // Assert
        var updated = await _repo.FindByIdAsync(vehicle.Id);
        Assert.Equal("NewPlate", updated?.LicensePlate);
    }

    [Fact]
    public async Task FindPagedAsync_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 10; i++)
        {
            await _repo.AddAsync(new Vehicle { LicensePlate = $"V{i}" });
        }

        // Act
        var page1 = await _repo.FindPagedAsync(1, 3);
        var page2 = await _repo.FindPagedAsync(2, 3);
        var page4 = await _repo.FindPagedAsync(4, 3);

        // Assert
        Assert.Equal(3, page1.Items.Count);
        Assert.Equal(3, page2.Items.Count);
        Assert.Single(page4.Items);
        Assert.Equal(10, page1.TotalCount);
        Assert.Equal(4, page1.TotalPages);
        Assert.True(page1.HasNext);
        Assert.False(page1.HasPrevious);
        Assert.True(page2.HasPrevious);
    }
}
