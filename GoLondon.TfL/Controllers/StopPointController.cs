using System.Net;
using GoLondon.Standard.Models.TfL;
using GoLondon.TfL.Services.Domain.Exceptions;
using GoLondon.TfL.Services.Domain.TfL;
using Microsoft.AspNetCore.Mvc;

namespace GoLondon.TfL.Controllers;


[ApiController]
[Route("TfL/[controller]")]
public class StopPointController : ControllerBase
{
    private readonly IStopPointService _stopPointService;
    private readonly ILogger _logger;
    public StopPointController(IStopPointService stopPointService, ILogger<StopPointController> logger)
    {
        _stopPointService = stopPointService;
        _logger = logger;
    }

    /// <summary>
    /// Returns the StopPoint from TfL, if it exists
    /// </summary>
    /// <param name="id">The unique Id, o naptan id, of the stop point</param>
    [HttpGet("{id}")]
    [Produces(typeof(tfl_StopPoint))]
    public async Task<IActionResult> GetStopPointById(string id, CancellationToken ct = default)
    {
        try
        {
            return Ok(await _stopPointService.GetStopPoint(id, ct));
        }
        catch (NoStopPointException ex)
        {
            _logger.LogWarning("Stop point {id} not found, requested from endpoint", id);
            return BadRequest($"No stop point with id {id} was found");
        }
        catch (TooManyStopPointsException)
        {
            _logger.LogWarning("Stop point {id} returned > 1 result, requested from endpoint", id);
            return BadRequest(
                $"Requested id {id} returned more than one stop point. Please input the most specific Id");
        }
        
    }

    [HttpGet("Search/{lat:float}/{lon:float}")]
    [Produces(typeof(List<tfl_StopPoint>))]
    public async Task<IActionResult> GetStopPointsByRadius(float lat, float lon, string? lineModeQuery, float radius = 200, CancellationToken ct = default)
    {
        try
        {
            return Ok(await _stopPointService.GetByRadius(lat, lon, lineModeQuery, radius, ct));
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to search for stop point, {lat}, {lon}", lat, lon);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
    
    [HttpGet("Search/{name}")]
    [Produces(typeof(List<tfl_StopPoint>))]
    public async Task<IActionResult> GetStopPointsByName(string name, string? lineModeQuery, CancellationToken ct = default)
    {
        try
        {
            return Ok(await _stopPointService.GetByName(name, lineModeQuery, ct));
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to search for stop point, {name}", name);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{id}/EstimatedArrivalsDepartures")]
    [Produces(typeof(List<tfl_ArrivalDeparture>))]
    public async Task<IActionResult> GetStopPointArrivals(string id, CancellationToken ct = default)
    {
        try
        {
            var result = await _stopPointService.GetArrivals(id, ct);
            return new JsonResult(result);
        }
        catch (NoStopPointException)
        {
            return BadRequest("Invalid stop id provided");
        }
        catch (TooManyStopPointsException)
        {
            return BadRequest("The stop id was not complete or returned more than one stop point");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to get arrivals, {id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
    
}