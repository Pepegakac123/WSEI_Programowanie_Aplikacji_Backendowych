using AppCore.Models;
using AppCore.Repositories;
using AppCore.ValueObjects;

namespace Infrastructure.Memory;

public class MemoryCarRepository : ICarRepository
{
    public Task<Car?> FindById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Car>> FindAll()
    {
        throw new NotImplementedException();
    }

    public Task<Car> Add(Car entity)
    {
        throw new NotImplementedException();
    }

    public Task<Car> DeleteById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Car> Update(Car entity)
    {
        throw new NotImplementedException();
    }

    public async Task<Car?> FindByPlateNumber(string plate)
    {
        if (plate == "KR 1234T")
        {
            return new Car()
            {
                Id = 1,
                PlateNumber = PlateNumber.Of("KR 1234T"),
                Entry = DateTime.Now,
                Exit = null
            };
        }

        return null;
    }
}