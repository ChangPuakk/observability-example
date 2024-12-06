using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;


public class TraceActionFilter : IActionFilter
{
    private readonly ActivitySource _activitySource;
    private readonly Counter<int> _connectionCounter;

    public TraceActionFilter(ActivitySource activitySource, Meter meter)
    {
        _activitySource = activitySource;
        _connectionCounter = meter.CreateCounter<int>("connections", description: "Counts the number of connections");
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var traceAttribute = context.ActionDescriptor.EndpointMetadata.OfType<TraceAttribute>().FirstOrDefault();
        if (traceAttribute != null)
        {
            var activity = _activitySource.StartActivity(traceAttribute.OperationName, ActivityKind.Server);
            context.HttpContext.Items["Activity"] = activity;

            // Increment the connection counter
            _connectionCounter.Add(1);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Items["Activity"] is Activity activity)
        {
            activity.Stop();
        }
    }
}