using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GeoJSON.Text.Geometry;
using Microsoft.AspNetCore.Mvc;
using Unica.Core.Contracts;
using Varca.API.Resources;
using Varca.Domain.Models;
using Varca.Domain.Services;

namespace Varca.Pit.API.Controllers;

[ApiController]
[Route("api/v1/geo/pit")]
public class PitApiController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IPitService _pitService;
    private readonly ILogger<PitApiController> _logger;
    public PitApiController(ILogger<PitApiController> logger, IMapper mapper, IPitService pitService)
    {
        _logger = logger;
        _mapper = mapper;
        _pitService = pitService;
    }

    [HttpGet("ready")]
    public ActionResult AreYouReady() {
        return Ok();
    }

    [HttpPut("point/{radius}")]
    [Produces("application/json")]
    public async Task<ActionResult<PitNewResouce>> CreatePitByPoint(float radius,[FromBody] PitGeometryResource pitGeometryResource) 
    {
        var headers = Request.Headers;
        var accountId = headers.GetCommaSeparatedValues("Account-Id").First().Trim();
        if (string.IsNullOrEmpty(accountId)) {
            Forbid();
        }

        var pitGeometryNew = _mapper.Map<PitGeometryNew>(pitGeometryResource);
        pitGeometryNew.AccountId = accountId;
        pitGeometryNew.Radius = radius;
        var pitNewResult = await _pitService.CreateByPointAsync(pitGeometryNew);
        if (pitNewResult == null) {
            return BadRequest();
        }

        var pitNewResouce = _mapper.Map<PitNewResouce>(pitNewResult);
        var locations = $"api/v1/geo/search/pit/{pitNewResouce.PitId},api/v1/content/pit/{pitNewResouce.PitId}";
        return Created(locations, pitNewResouce);
    }

    [HttpPut("polygon")]
    [Produces("application/json")]
    public async Task<ActionResult<PitNewResouce>> CreatePitByPolygon([FromBody] PitGeometryResource pitGeometryResource) 
    {
        var headers = Request.Headers;
        var accountId = headers.GetCommaSeparatedValues("Account-Id").First().Trim();
        if (string.IsNullOrEmpty(accountId)) {
            Forbid();
        }

        var pitGeometryNew = _mapper.Map<PitGeometryNew>(pitGeometryResource);
        pitGeometryNew.AccountId = accountId;
        var pitNewResult = await _pitService.CreateByPolygonAsync(pitGeometryNew);
        if (pitNewResult == null) {
            return BadRequest();
        }

        var pitNewResouce = _mapper.Map<PitNewResouce>(pitNewResult);
        var locations = $"api/v1/geo/search/pit/{pitNewResouce.PitId},api/v1/content/pit/{pitNewResouce.PitId}";
        return Created(locations, pitNewResouce);
    }

    [HttpPut("place/{placeId}")]
    public async Task<ActionResult<PitNewResouce>> CreatePitByPlaceId(string placeId, [FromQuery] float radius, 
                        [FromQuery] int polygon, [FromBody] PitPlaceResource pitPlaceResource, [FromQuery] string source = "vervotech")
    {
        var headers = Request.Headers;
        var accountId = headers.GetCommaSeparatedValues("Account-Id").First().Trim();
        if (string.IsNullOrEmpty(accountId)) {
            Forbid();
        }

        var pitPlaceNew = _mapper.Map<PitPlaceNew>(pitPlaceResource);
        pitPlaceNew.AccountId = accountId;
        pitPlaceNew.Source = source;
        pitPlaceNew.Radius = radius;
        pitPlaceNew.PlaceId = placeId;
        pitPlaceNew.Polygon = polygon;

        var pitNewResult = await _pitService.CreateByPlaceAsync(pitPlaceNew);
        if (pitNewResult == null) {
            return BadRequest();
        }

        var pitNewResouce = _mapper.Map<PitNewResouce>(pitNewResult);
        var locations = $"api/v1/geo/search/pit/{pitNewResouce.PitId},api/v1/content/pit/{pitNewResouce.PitId}";
        return Created(locations, pitNewResouce);
    }

    [HttpGet("{pitId}")]
    public async Task<ActionResult<PitInfoResouce>> GetPitInfo(string pitId)
    {
        var headers = Request.Headers;
        var accountId = headers.GetCommaSeparatedValues("Account-Id").First().Trim();
        if (string.IsNullOrEmpty(accountId)) {
            Forbid();
        }

        var pitInfo = await _pitService.GetByIdAsync(pitId, accountId);
        if (pitInfo == null) {
            return NotFound();
        }

        var pitInfoResouce = _mapper.Map<PitInfoResouce>(pitInfo);
        return Ok(pitInfoResouce);
    } 

}
