using Microsoft.AspNetCore.Http;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

public class TraceMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ActivitySource _activitySource;

  public TraceMiddleware(RequestDelegate next, ActivitySource activitySource)
  {
    _next = next;
    _activitySource = activitySource;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    var endpoint = context.GetEndpoint();
    if (endpoint != null)
    {
      var controllerActionDescriptor = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>();
      if (controllerActionDescriptor != null)
      {
        var controllerType = controllerActionDescriptor.ControllerTypeInfo.AsType();
        if (controllerType.GetCustomAttribute<TraceAttribute>() != null)
        {
          var activity = _activitySource.StartActivity(controllerType.Name);

          try
          {
            activity?.SetTag("controller", controllerType.Name);
            activity?.SetTag("method", controllerActionDescriptor.ActionName);
            await _next(context);
          }
          finally
          {
            activity?.Stop();
          }
          return;
        }
      }
    }

    await _next(context);
  }
}