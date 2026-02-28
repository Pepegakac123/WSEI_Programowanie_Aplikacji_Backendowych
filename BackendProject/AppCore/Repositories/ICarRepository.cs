namespace AppCore.Repositories;
using AppCore.Models;

public interface ICarRepository: IGenericRepository<Car>
{
    Task<Car?> FindByPlateNumber(string plate);
}