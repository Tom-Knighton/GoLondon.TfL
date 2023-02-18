using GoLondon.Standard.Models.TfL;
using GoLondon.TfL.Services.Domain.Exceptions;
using GoLondon.TfL.Services.Domain.ServiceCollections.TfL;
using GoLondon.TfL.Services.Domain.TfL;

namespace GoLondon.TfL.Services.TfL;

public class StopPointService : IStopPointService
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
}