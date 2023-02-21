using GoLondon.Standard.Models.TfL;

namespace GoLondon.TfL.Services.Domain.TfL;

public interface IStopPointService
{
    public Task<tfl_StopPoint> GetStopPoint(string Id, CancellationToken ct);

    public Task<List<tfl_StopPoint>> GetByRadius(float lat, float lon, string? lineModeQuery, float radius,
        CancellationToken ct);

    public Task<List<tfl_StopPoint>> GetByName(string name, string? lineModeQuery, CancellationToken ct);

    public Task<tfl_ArrivalDepartureParent[]> GetArrivals(string id, CancellationToken ct);
}