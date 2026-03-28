using System;
using System.Linq;
using System.Threading.Tasks;
using AppCore.Models;
using AppCore.Repositories;

namespace Infrastructure.Repositories;

public class InMemoryParkingGateRepository : MemoryGenericRepository<ParkingGate>, IParkingGateRepository
{
    public InMemoryParkingGateRepository()
    {
        var gate1 = new ParkingGate()
        {
            Id = Guid.NewGuid(),
            Name = "Entry Gate",
            Type = GateType.Entry,
            Location = "Main Gate",
            IsOperational = false
        };
        _data.Add(gate1.Id, gate1);
        
        var gate2 = new ParkingGate()
        {
            Id = Guid.NewGuid(),
            Name = "Exit Gate",
            Type = GateType.Exit,
            Location = "Back Gate",
            IsOperational = true
        };
        _data.Add(gate2.Id, gate2);
    }

    public Task<ParkingGate?> FindByNameAsync(string name)
    {
        var result = _data.Values.FirstOrDefault(g => g.Name == name);
        return Task.FromResult(result);
    }
}