using AutoMapper;
using AppCore.Dto;
using AppCore.Models;

namespace AppCore.Mapper;

public class ParkingTariffMappingProfile : Profile
{
    public ParkingTariffMappingProfile()
    {
        CreateMap<ParkingTariff, ParkingTariffDto>();
        
        CreateMap<CreateTariffDto, ParkingTariff>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.FreeParkingDuration, opt => opt.MapFrom(src => TimeSpan.FromMinutes(src.FreeMinutes)))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => false));
    }
}