using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Varca.API.Resources;
using Varca.Domain.Models;

namespace Varca.Place.API;

public class AutoMappingProfile : Profile
{
    public AutoMappingProfile() 
    {
        CreateMap<FilterResource, Filter>();
        CreateMap<GeoHotelMapping, MappingResource>()
            .ForMember(dest => dest.VervotechId, opt => opt.MapFrom(src => src.UnicaId))
            .ForMember(dest => dest.ProviderName, opt => opt.MapFrom(src => src.ProviderFamily));

        CreateMap<GeoHotelMappingCollection, MappingCollectionResource>();
    }
}
