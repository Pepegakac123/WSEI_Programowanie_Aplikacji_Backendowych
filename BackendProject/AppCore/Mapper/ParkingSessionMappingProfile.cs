using AutoMapper;
using AppCore.Dto;
using AppCore.Models;

namespace AppCore.Mapper;

public class ParkingSessionMappingProfile : Profile
{
    public ParkingSessionMappingProfile()
    {
        CreateMap<ParkingSession, ActiveParkingSessionDto>()
            .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CurrentDuration, opt => opt.MapFrom(src => DateTime.Now - src.EntryTime));

        CreateMap<ParkingSession, ParkingSessionHistoryDto>()
            .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.ExitTime.HasValue ? src.ExitTime - src.EntryTime : null));
    }
}