using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GoLondon.Standard.Models.TfL;
using GoLondon.TfL.Services.Domain.Exceptions;
using GoLondon.TfL.Services.Domain.Models;

namespace GoLondon.TfL.Services.Domain.ServiceCollections.TfL;

public class TflApiClient : ITfLApiClient
{
    private readonly HttpClient _client;
    private const string ClientName = "TfLApiClient";

    public TflApiClient(IHttpClientFactory clientFactory)
    {
        _client = clientFactory.CreateClient(ClientName);
    }
    
    public async Task<tfl_StopPoint> GetStopPointByIdAsync(string id, CancellationToken ct)
    {
        var response = await _client.GetAsync($"StopPoint/{id}", ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync(ct);
        if (result.FirstOrDefault() == '[')
        {
            throw new TooManyStopPointsException();
        }
        return JsonSerializer.Deserialize<tfl_StopPoint>(result);
    }

    public async Task<List<tfl_StopPoint>> GetByRadius(float lat, float lon, string? lineModeQuery, float radius, CancellationToken ct)
    {
        var url = $"StopPoint/?lat={lat}&lon={lon}&radius={radius}&stoptypes=NaptanMetroStation,NaptanRailStation,NaptanBusCoachStation,NaptanFerryPort,NaptanPublicBusCoachTram";
        if (!string.IsNullOrEmpty(lineModeQuery))
        {
            url += $"&modes={lineModeQuery}";
        }
        var response = await _client.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync(ct);
        var searchResponse = JsonSerializer.Deserialize<tfl_SearchResponse>(result);
        return searchResponse?.stopPoints;
    }

    public async Task<List<tfl_StopPoint>> GetByName(string name, string? lineModeQuery, CancellationToken ct)
    {
        var url = $"StopPoint/Search?query={name}";
        if (!string.IsNullOrEmpty(lineModeQuery))
        {
            url += $"&modes={lineModeQuery}";
        }
        var response = await _client.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync(ct);
        var searchResponse = JsonSerializer.Deserialize<tfl_SearchNameResponse>(result);
        return searchResponse?.matches;
    }

    

    public async Task<List<tfl_ArrivalDeparture>> GetLOELZDepartures(string lineMode, string stopPointId, CancellationToken ct)
    {
        var url = $"StopPoint/{stopPointId}/ArrivalDepartures?lineIds={lineMode}";
        var response = await _client.GetAsync(url, ct);
        Console.WriteLine(url + " " + response.StatusCode);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync(ct);
        var arrivalResponse = JsonSerializer.Deserialize<List<tfl_ArrivalDeparture>>(result) ?? new();
        return arrivalResponse;
    }

    public async Task<List<tfl_NonRailArrival>> GetArrivals(string stopPointId, CancellationToken ct)
    {
        var url = $"StopPoint/{stopPointId}/Arrivals";
        var response = await _client.GetAsync(url, ct);
        Console.WriteLine(url + " " + response.StatusCode);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync(ct);
        var arrivalResponse = JsonSerializer.Deserialize<List<tfl_NonRailArrival>>(result);
        return arrivalResponse ?? new List<tfl_NonRailArrival>();
    }
}
