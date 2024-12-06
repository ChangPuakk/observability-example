using Microsoft.AspNetCore.Http;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;

public class ConnectionMetricsMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly Meter Meter = new Meter("Application1.Metrics");
    private static readonly Counter<long> ConnectionCounter = Meter.CreateCounter<long>("connections", "Counts the number of connections");

    public ConnectionMetricsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Increment the connection counter
        ConnectionCounter.Add(1);
        // ConnectionCounter.Add(-1);
        await _next(context);
    }
}