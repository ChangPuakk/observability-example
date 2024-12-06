using Microsoft.AspNetCore.Mvc.Filters;
using OpenTelemetry.Trace;
using System.Diagnostics;

public class TraceActionFilter : IActionFilter
{
    private readonly ActivitySource _activitySource;

    public TraceActionFilter(ActivitySource activitySource)
    {
        _activitySource = activitySource;
    
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var traceAttribute = context.ActionDescriptor.EndpointMetadata.OfType<TraceAttribute>().FirstOrDefault();
        if (traceAttribute != null)
        {
            var activity = _activitySource.StartActivity(traceAttribute.OperationName, ActivityKind.Server);
            context.HttpContext.Items["Activity"] = activity;
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