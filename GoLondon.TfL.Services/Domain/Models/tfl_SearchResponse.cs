using GoLondon.Standard.Models.TfL;

namespace GoLondon.TfL.Services.Domain.Models;

public class tfl_SearchResponse
{
    public List<tfl_StopPoint> stopPoints { get; set; }
    public float[] centrePoint { get; set; }
}

public class tfl_SearchNameResponse
{
    public List<tfl_StopPoint> matches { get; set; }
}