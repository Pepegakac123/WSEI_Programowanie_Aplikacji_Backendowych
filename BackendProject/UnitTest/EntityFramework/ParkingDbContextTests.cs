using AppCore.Models;
using AppCore.Services;
using Infrastructure.EntityFramework.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UnitTest.Services;
using Xunit;

namespace UnitTest.EntityFramework;

public class ParkingDbContextTests
{
    [Fact]
    public async Task SaveChangesAsync_ShouldAutomaticallyAssignCreatedByUserId()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ParkingDbContext>()
            .UseSqlite(connection)
            .Options;
        
        ICurrentUserService fakeUserService = new TestCurrentUserService(); 
        
        using var context = new ParkingDbContext(options, fakeUserService);
        context.Database.EnsureCreated();


        var newGate = new ParkingGate { Name = "Brama Testowa", Type = GateType.Entry };


        context.ParkingGate.Add(newGate);
        await context.SaveChangesAsync();
        
        Assert.Equal("test-user-id", newGate.CreatedByUserId);
        
        Assert.NotEqual(Guid.Empty, newGate.Id);
    }
}