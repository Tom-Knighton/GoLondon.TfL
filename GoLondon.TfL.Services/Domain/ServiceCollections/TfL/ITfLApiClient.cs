using GoLondon.Standard.Models.TfL;

namespace GoLondon.TfL.Services.Domain.ServiceCollections.TfL;

public interface ITfLApiClient
{
    public Task<tfl_StopPoint> GetStopPointByIdAsync(string id, CancellationToken ct);
    public Task<List<tfl_StopPoint>> GetByRadius(float lat, float lon, string? lineModeQuery, float radius,
        CancellationToken ct);
    public Task<List<tfl_StopPoint>> GetByName(string name, string? lineModeQuery, CancellationToken ct);
}