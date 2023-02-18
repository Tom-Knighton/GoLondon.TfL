using GoLondon.Standard.Models.TfL;

namespace GoLondon.TfL.Services.Domain.TfL;

public interface IStopPointService
{
    public Task<tfl_StopPoint> GetStopPoint(string Id, CancellationToken ct);
}