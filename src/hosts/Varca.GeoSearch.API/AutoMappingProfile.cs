using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Varca.API.Resources;
using Varca.Domain.Models;

namespace Varca.GeoSearch.API;

public class AutoMappingProfile : Profile
{
    public AutoMappingProfile() 
    {
        CreateMap<FilterResource, Filter>();
        CreateMap<ProviderResource, ProviderFilter>();
        CreateMap<SearchGeometryResource, SearchMappingsGeometry>();
        CreateMap<SearchPlaceResource, SearchMappingsPlace>();
        
        CreateMap<GeoHotelMapping, MappingResource>()
            .ForMember(dest => dest.VervotechId, opt => opt.MapFrom(src => src.UnicaId))
            .ForMember(dest => dest.ProviderName, opt => opt.MapFrom(src => src.ProviderFamily));

        CreateMap<GeoHotelMappingCollection, MappingCollectionResource>();
        CreateMap<PitContextResource, PitContext>();
        CreateMap<PitContext, PitContextResource>();
    }
}
