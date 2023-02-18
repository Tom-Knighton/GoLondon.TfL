using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoLondon.TfL.Services.Domain.ServiceCollections.TfL;

public static class TfLApiCollection
{

    public static IServiceCollection AddTfLApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<TfLAccessHandler>(h =>
            new TfLAccessHandler(configuration.GetValue<string>("id"), configuration.GetValue<string>("token")));

        services.AddTransient<ITfLApiClient, TflApiClient>();
        services.AddHttpClient("TfLApiClient", c =>
        {
            c.BaseAddress = new Uri(configuration.GetValue<string>("Host")!);
        })
        .AddHttpMessageHandler<TfLAccessHandler>();

        return services;
    }
}

public class TfLAccessHandler : DelegatingHandler
{
    private readonly string _appId;
    private readonly string _appToken;

    public TfLAccessHandler(string? token, string? id) : base()
    {
        _appId = Uri.EscapeDataString(id ?? "");
        _appToken = Uri.EscapeDataString(token ?? "");
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RequestUri?.ToString()))
            throw new ArgumentNullException(nameof(request.RequestUri), "The request's URI was null or invalid");
        
        var uriBuilder = new UriBuilder(request.RequestUri);
        uriBuilder.Query += $"app_id={_appId}&app_key={_appToken}";
        request.RequestUri = uriBuilder.Uri;

        return base.SendAsync(request, cancellationToken);
    }
}