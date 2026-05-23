using AutoMapper;
using AppCore.Dto;
using AppCore.Models;

namespace AppCore.Mapper;

public class VehicleMappingProfile : Profile
{
    public VehicleMappingProfile()
    {
        CreateMap<Vehicle, VehicleDto>();
    }
}