using GoLondon.Standard.Models.TfL;

namespace GoLondon.TfL.Services.Domain.ServiceCollections.TfL;

public interface ITfLApiClient
{
    public Task<tfl_StopPoint> GetStopPointByIdAsync(string id, CancellationToken ct);
}