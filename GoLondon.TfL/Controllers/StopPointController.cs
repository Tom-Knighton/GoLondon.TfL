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
    public StopPointController(IStopPointService stopPointService)
    {
        _stopPointService = stopPointService;
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
            return BadRequest($"No stop point with id {id} was found");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}