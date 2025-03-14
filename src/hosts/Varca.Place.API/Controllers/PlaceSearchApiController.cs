using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;
using Microsoft.AspNetCore.Mvc;
using Unica.Core.Contracts;
using Varca.API.Resources;
using Varca.Domain.Models;
using Varca.Domain.Services;

namespace Varca.Place.API.Controllers;

[ApiController]
[Route("/api/v1/place")]
public class PlaceSearchApiController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IPlaceSearchService _placeSearchService;
    private readonly ILogger<PlaceSearchApiController> _logger;


    public PlaceSearchApiController(ILogger<PlaceSearchApiController> logger, IMapper mapper, 
                                        IPlaceSearchService placeSearchService) {
        _mapper = mapper;  
        _placeSearchService = placeSearchService;
        _logger = logger;
    }

    [HttpGet("ready")]
    public ActionResult AreYouReady() {
        return Ok();
    }

    [HttpGet("search/{keyword}")]
    [Produces("application/json")]
    public async Task<ActionResult<FeatureCollection>> SearchByKeywordAsync(string keyword, 
                                                                [FromQuery] GeoPlaceSource source,
                                                                [FromQuery] int limit = 20)
    {
        var headers = Request.Headers;
        var accountId = headers.GetCommaSeparatedValues("Account-Id").First().Trim();

        if (string.IsNullOrEmpty(accountId)) {
            Forbid();
        }

        var query = new PlaceKeywordSearchQuery {
            AccountId = accountId,
            Keyword = keyword,
            Limit = limit,
            Source = source
        };

        var featureCollection = await _placeSearchService.SearchByKeywordAsync(query);
        if (featureCollection == null) {
            NotFound();
        }

        return Ok(featureCollection);
    }
    

    [HttpGet("{placeId}")]
    [Produces("application/json")]
    public async Task<ActionResult<Feature>> GetByPlaceIdAsync(string placeId, [FromQuery] GeoPlaceSource source) {
        var headers = Request.Headers;
        var accountId = headers.GetCommaSeparatedValues("Account-Id").First().Trim();

        if (string.IsNullOrEmpty(accountId)) {
            Forbid();
        }

        var query = new PlaceIdQuery {
            AccountId = accountId,
            PlaceId = placeId,
            Source = source
        };

        var feature = await _placeSearchService.GetByPlaceIdAsync(query);
        if (feature == null) {
            return NotFound();
        }
        return Ok(feature);
    }
}
