using AutoMapper;
using AppCore.Dto;
using AppCore.Models;

namespace AppCore.Mapper;

public class GateMappingProfile : Profile
{
    public GateMappingProfile()
    {
        CreateMap<ParkingGate, ParkingGateDto>()
            .ForMember(
                dest => dest.Type,
                opt => opt.MapFrom(
                    src => src.Type.ToString()));
    }
}
