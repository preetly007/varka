using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GeoJSON.Text;
using Microsoft.AspNetCore.Mvc;
using Unica.Core.Contracts;
using Varca.API.Resources;
using Varca.Domain.Models;
using Varca.Domain.Services;

//GET  https://hotelmappings.vervotech.com/api/geo/search/pit/01HV8K3XNV346NJ8CCSJKZZTK9
//POST https://hotelmappings.vervotech.com/api/geo/search/point/5.3
//POST https://hotelmappings.vervotech.com/api/geo/search/polygon
//POST https://hotelmappings.vervotech.com/api/geo/search/place/{placeId}?radis=5.2&polygon=1&source=vervotech


namespace Varca.GeoSearch.API.Controllers;


[ApiController]
[Route("/api/v1/geo/search")]
public class GeoMappingApiController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IGeoHotelMappingService _geoMappingService;
    private readonly ILogger<GeoMappingApiController> _logger;

    public GeoMappingApiController(ILogger<GeoMappingApiController> logger, IGeoHotelMappingService geoMappingService, IMapper mapper)
    {
        _mapper = mapper;
        _geoMappingService = geoMappingService;
        _logger = logger;
    }

    [HttpGet("ready")]
    public ActionResult AreYouReady() {
        return Ok();
    }

    [HttpGet("pit/{pitId}")]
    [Produces("application/json")]
    public async Task<ActionResult<MappingCollectionResource>> GetByPit(string pitId) 
    {
        var headers = Request.Headers;
        var accountId = headers.GetCommaSeparatedValues("Account-Id").First().Trim();
        if (string.IsNullOrEmpty(accountId)) {
            Forbid();
        }

        var searchQuery = new SearchMappingsPit {
            AccountId = accountId,
            PitId = pitId
        };

        var geoHotelMappings = await _geoMappingService.SearchByPitAsync(searchQuery);
         if (geoHotelMappings == null)
            return NotFound();

        if (geoHotelMappings.Mappings == null || geoHotelMappings.Mappings.Count == 0)
            return NoContent();

        var mappingsResource = _mapper.Map<MappingCollectionResource>(geoHotelMappings);
        return Ok(mappingsResource);
    }

    [HttpPost("point/{radius}")]
    [Produces("application/json")]
    public async Task<ActionResult<MappingCollectionResource>> SearchByPointAsync(float radius, 
                                                                            [FromBody] SearchGeometryResource searchGeometryResource)
    {
        var geometryQuery = _mapper.Map<SearchMappingsGeometry>(searchGeometryResource);

        var headers = Request.Headers;
        var accountId = headers.GetCommaSeparatedValues("Account-Id").First().Trim();
        if (string.IsNullOrEmpty(accountId)) {
            Forbid();
        }

        geometryQuery.AccountId = accountId;
        geometryQuery.Radius = radius;

        var geoHotelMappings = await _geoMappingService.SearchByPointAsync(geometryQuery);
        if (geoHotelMappings == null)
            return BadRequest();

        var mappingsResource = _mapper.Map<MappingCollectionResource>(geoHotelMappings);
        return Ok(mappingsResource);
    }

    [HttpPost("polygon")]
    [Produces("application/json")]
    public async Task<ActionResult<MappingCollectionResource>> SearchByPolygonAsync([FromBody] SearchGeometryResource searchGeometryResource)
    {
        var geometryQuery = _mapper.Map<SearchMappingsGeometry>(searchGeometryResource);
        var headers = Request.Headers;
        var accountId = headers.GetCommaSeparatedValues("Account-Id").First();
        geometryQuery.AccountId = accountId;

        GeoHotelMappingCollection? geoHotelMappings;
        if (geometryQuery.Geometry.Type == GeoJSONObjectType.Polygon) {
            geoHotelMappings = await _geoMappingService.SearchByPolygonAsync(geometryQuery);
        } else if (geometryQuery.Geometry.Type == GeoJSONObjectType.MultiPolygon) {
            geoHotelMappings = await _geoMappingService.SearchByMultipolygonAsync(geometryQuery);
        } else {
            return BadRequest();
        }

        if (geoHotelMappings == null)
            return BadRequest();

        var mappingsResource = _mapper.Map<MappingCollectionResource>(geoHotelMappings);
        return Ok(mappingsResource);
    }

    [HttpPost("place/{placeId}")]
    [Produces("application/json")]
    public async Task<ActionResult<MappingCollectionResource>> SearchByPlaceAsync(string placeId, [FromBody] SearchPlaceResource? searchPlaceResource, 
                                                                [FromQuery] float radius, 
                                                                [FromQuery] int polygon, [FromQuery] string source = "vervotech")
    {
        searchPlaceResource ??= new SearchPlaceResource();
        var searchPlaceQuery = _mapper.Map<SearchMappingsPlace>(searchPlaceResource);
        
        var headers = Request.Headers;
        var accountId = headers.GetCommaSeparatedValues("Account-Id").First().Trim();
        if (string.IsNullOrEmpty(accountId)) {
            Forbid();
        }
        
        searchPlaceQuery.AccountId = accountId;
        searchPlaceQuery.PlaceId = placeId;
        searchPlaceQuery.Polygon = polygon;
        searchPlaceQuery.Source = "vervotech";//source;
        searchPlaceQuery.Radius = radius;

        var geoHotelMappings = await _geoMappingService.SearchByPlaceAsync(searchPlaceQuery);
        if (geoHotelMappings == null)
            return NotFound();
            
        var mappingsResource = _mapper.Map<MappingCollectionResource>(geoHotelMappings);
        return Ok(mappingsResource);
        
    }
}
