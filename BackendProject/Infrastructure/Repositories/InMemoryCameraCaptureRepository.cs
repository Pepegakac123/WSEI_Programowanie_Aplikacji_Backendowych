using AppCore.Models;
using AppCore.Repositories;

namespace Infrastructure.Repositories;

public class InMemoryCameraCaptureRepository : MemoryGenericRepository<CameraCapture>, ICameraCaptureRepository
{
    
}
