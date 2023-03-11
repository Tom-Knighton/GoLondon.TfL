using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;
using GoLondon.Standard.Models.TfL;
using GoLondon.TfL.Services.Domain.Exceptions;
using GoLondon.TfL.Services.Domain.ServiceCollections.TfL;
using GoLondon.TfL.Services.Domain.TfL;
using Microsoft.Extensions.Logging;

namespace GoLondon.TfL.Services.TfL;

public partial class StopPointService : IStopPointService
{
    private readonly ILogger _logger;
    private readonly ITfLApiClient _apiClient;
    
    public StopPointService(ITfLApiClient apiClient, ILogger<StopPointService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }
    
    public async Task<tfl_StopPoint> GetStopPoint(string id, CancellationToken ct)
    {
        try
        {
            var stopPoint = await _apiClient.GetStopPointByIdAsync(id, ct);
            if (stopPoint is null)
                throw new NoStopPointException();

            return stopPoint;
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NoStopPointException();
            }
            _logger.LogError(ex, "Failed to get stop point {id}", id);
            throw new ApiException("An unknown error occurred processing this request");
        }
    }

    public async Task<List<tfl_StopPoint>> GetByRadius(float lat, float lon, string lineModeQuery, float radius, CancellationToken ct)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(lineModeQuery) &&
                !CSVListQuery().IsMatch(lineModeQuery))
                throw new ValidationException("Invalid lineModeQuery, must be comma separated list of line modes, or null");
            
            var stopPoints = await _apiClient.GetByRadius(lat, lon, lineModeQuery, radius, ct);
            if (!stopPoints.Any())
                throw new NoStopPointException();

            return stopPoints;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get stop points by radius {lat}, {lon}", lat, lon);
            throw new ApiException("An unknown error occurred processing this request");
        }
    }

    public async Task<List<tfl_StopPoint>> GetByName(string name, string? lineModeQuery, CancellationToken ct)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(lineModeQuery) &&
                !CSVListQuery().IsMatch(lineModeQuery))
            {
                _logger.LogWarning("GetByName query was invalid: {query}", lineModeQuery);
                throw new ValidationException("Invalid lineModeQuery, must be comma separated list of line modes, or null");
            }

            var stopPoints = await _apiClient.GetByName(name, lineModeQuery, ct);
            if (!stopPoints.Any())
                throw new NoStopPointException();

            return stopPoints;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get stop points name: {name}", name);
            throw new ApiException("An unknown error occurred processing this request");
        }
    }

    public async Task<tfl_ArrivalDepartureParent[]> GetArrivals(string id, CancellationToken ct)
    {
        try
        {
            var stopPoint = await GetStopPoint(id, ct);
            if (stopPoint is null)
                throw new NoStopPointException();

            List<Task<List<tfl_ArrivalDeparture>>> arrivalDepartureTasks = new();
            List<Task<List<tfl_NonRailArrival>>> arrivalsTasks = new();
            List<string> collectedStopArrivals = new();

            //Step 1: Recursively get relevant child arrivals/departures
            GetChildArrivals(stopPoint, arrivalDepartureTasks, arrivalsTasks, collectedStopArrivals, ct);

            //Step 2: Get direct LO/ELZ departures if at HUB
            if (stopPoint.id?.StartsWith("HUB") == false)
            {
                if (stopPoint.lineModeGroups?.Any(m => m.modeName == "overground") == true)
                {
                    arrivalDepartureTasks.Add(_apiClient.GetLOELZDepartures("london-overground", stopPoint.id, ct));
                }

                if (stopPoint.lineModeGroups?.Any(m => m.modeName == "elizabeth-line") == true)
                {
                    arrivalDepartureTasks.Add(_apiClient.GetLOELZDepartures("elizabeth", stopPoint.id, ct));
                }
            }

            //Step 3: If we haven't observed the parent stop point yet, get arrivals there
            if (!collectedStopArrivals.Contains(stopPoint.id ?? ""))
            {
                arrivalsTasks.Add(_apiClient.GetArrivals(stopPoint.id ?? "", ct));
                collectedStopArrivals.Add(stopPoint.id ?? "");
            }

            //Step 4: Run all tasks and remove final stops from LO/ELZ trains
            var arrivalDepartures = Array.Empty<tfl_ArrivalDepartureParent>();
            arrivalDepartures = arrivalDepartures.Concat((await Task.WhenAll(arrivalDepartureTasks)).SelectMany(l => l))
                .ToArray();
            arrivalDepartures = arrivalDepartures.Concat((await Task.WhenAll(arrivalsTasks)).SelectMany(l => l))
                .ToArray();

            arrivalDepartures =
                arrivalDepartures
                    .Where(a => (a is tfl_ArrivalDeparture &&
                                 stopPoint.children?.Any(c => c.id == a.destinationNaptanId) == false) ||
                                a is not tfl_ArrivalDeparture)
                    .OrderBy(a => GetOrderByDate(a))
                    .ToArray();
            return arrivalDepartures;

        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get arrivals for {id}", id);
            throw new ApiException("An unknown error occurred processing this request");
        }
        catch (NoStopPointException ex)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get arrivals for {id}", id);
            throw new Exception("An error occurred", ex);
        }
    }

    private void GetChildArrivals(tfl_StopPoint stopPoint, List<Task<List<tfl_ArrivalDeparture>>> departuresTasks,
        List<Task<List<tfl_NonRailArrival>>> arrivalsTasks, List<string> collectedArrivals, CancellationToken ct)
    {
        stopPoint.children?.Where(c => c.lineModeGroups?.Any() == true).ToList().ForEach(child =>
        {
            // If is LO or ELZ, get their departures
            if (child.lineModeGroups != null && child.lineModeGroups.Any(m => m.modeName is "overground" or "elizabeth-line"))
            {
                departuresTasks.Add(_apiClient.GetLOELZDepartures("london-overground", child.id, ct));
                departuresTasks.Add(_apiClient.GetLOELZDepartures("elizabeth", child.id, ct));
            }
            else
            {
                child.lineGroup?.Where(lg => lg.naptanIdReference != null).ToList().ForEach(lg =>
                {
                    if (lg.naptanIdReference == null || collectedArrivals.Contains(lg.naptanIdReference)) return;
                    // Get the arrivals and add the naptan id to the collected list
                    arrivalsTasks.Add(_apiClient.GetArrivals(lg.naptanIdReference, ct));
                    collectedArrivals.Add(lg.naptanIdReference);
                });

                if (child.lineModeGroups?.Any(m => m.modeName == "bus") == true && child.stopLetter == null &&
                    child.indicator == null)
                {
                    GetChildArrivals(child, departuresTasks, arrivalsTasks, collectedArrivals, ct);
                }
                else if (collectedArrivals.Contains(child.id) == false)
                {
                    arrivalsTasks.Add(_apiClient.GetArrivals(child.id, ct));
                    collectedArrivals.Add(child.id);
                }
            }
        });
    }

    private DateTime? GetOrderByDate(tfl_ArrivalDepartureParent arrival)
    {
        return arrival switch
        {
            tfl_ArrivalDeparture arrivalDeparture => arrivalDeparture.scheduledTimeOfDeparture ??
                                                     arrivalDeparture.scheduledTimeOfArrival ??
                                                     arrivalDeparture.estimatedTimeOfDeparture ??
                                                     arrivalDeparture.estimatedTimeOfArrival,
            tfl_NonRailArrival nrArrival => nrArrival.expectedArrival,
            _ => null
        };
    }

    [GeneratedRegex("^(?:[a-zA-Z0-9-]+(?:,[a-zA-Z0-9-]+)*|\\s*)$")]
    private static partial Regex CSVListQuery();
}