using AppCore.Models;
using AppCore.Repositories;
using Infrastructure.EntityFramework.Context;

namespace Infrastructure.EntityFramework.Repositories;

public class EfCameraCaptureRepository(ParkingDbContext context) : EfGenericRepository<CameraCapture>(context.CameraCapture), ICameraCaptureRepository
{
    
}