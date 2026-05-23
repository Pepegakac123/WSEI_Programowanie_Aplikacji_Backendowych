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

        CreateMap<CreateGateDto, ParkingGate>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(
                dest => dest.Type,
                opt => opt.MapFrom(src => Enum.Parse<GateType>(src.Type, true)))
            .ForMember(
                dest => dest.IsOperational,
                opt => opt.MapFrom(_ => false))
            .ForMember(
                dest => dest.CameraCaptures,
                opt => opt.MapFrom(_ => new List<CameraCapture>()));
    }
}
