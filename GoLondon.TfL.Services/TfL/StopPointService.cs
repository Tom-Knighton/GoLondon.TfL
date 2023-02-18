using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using GoLondon.Standard.Models.TfL;
using GoLondon.TfL.Services.Domain.Exceptions;
using GoLondon.TfL.Services.Domain.ServiceCollections.TfL;
using GoLondon.TfL.Services.Domain.TfL;

namespace GoLondon.TfL.Services.TfL;

public partial class StopPointService : IStopPointService
{
    private readonly ITfLApiClient _apiClient;
    
    public StopPointService(ITfLApiClient apiClient)
    {
        _apiClient = apiClient;
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
            throw new ApiException("An unknown error occurred processing this request");
        }
    }

    [GeneratedRegex("^(?:[a-zA-Z0-9-]+(?:,[a-zA-Z0-9-]+)*|\\s*)$")]
    private static partial Regex CSVListQuery();
}