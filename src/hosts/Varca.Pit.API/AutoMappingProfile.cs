using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Varca.API.Resources;
using Varca.Domain.Models;

namespace Varca.Pit.API;

public class AutoMappingProfile : Profile
{
    public AutoMappingProfile() 
    {
        CreateMap<FilterResource, Filter>();

        CreateMap<GeoHotelMapping, MappingResource>()
            .ForMember(dest => dest.VervotechId, opt => opt.MapFrom(src => src.UnicaId))
            .ForMember(dest => dest.ProviderName, opt => opt.MapFrom(src => src.ProviderFamily));

        CreateMap<GeoHotelMappingCollection, MappingCollectionResource>();
        CreateMap<PitGeometryResource,PitGeometryNew>();
        CreateMap<PitGeometryNew, PitNewResouce>();
        CreateMap<FilterResource, Filter>();
        CreateMap<ProviderResource, ProviderFilter>();
        CreateMap<PitPlaceResource, PitPlaceNew>();
        CreateMap<PitPlaceNew, PitNewResouce>();
        CreateMap<PitInfo, PitInfoResouce>();
        CreateMap<PitContextResource, PitContext>();
        CreateMap<PitContext, PitContextResource>();
    }
}
