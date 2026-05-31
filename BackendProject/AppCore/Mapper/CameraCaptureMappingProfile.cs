using AutoMapper;
using AppCore.Dto;
using AppCore.Models;

namespace AppCore.Mapper;

public class CameraCaptureMappingProfile : Profile
{
    public CameraCaptureMappingProfile()
    {
        CreateMap<CameraCapture, CameraCaptureDto>()
            .ForCtorParam("Brand", opt => opt.MapFrom(src => src.DetectedBrand))
            .ForCtorParam("Color", opt => opt.MapFrom(src => src.DetectedColor));
        
        CreateMap<CreateCameraCaptureDto, CameraCapture>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CapturedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.DetectedBrand, opt => opt.MapFrom(src => src.Brand))
            .ForMember(dest => dest.DetectedColor, opt => opt.MapFrom(src => src.Color))
            .ForMember(dest => dest.CaptureType, opt => opt.MapFrom(src => Enum.Parse<CaptureType>(src.CaptureType, true)));
    }
}