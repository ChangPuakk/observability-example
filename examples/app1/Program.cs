using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var ApplicationName = "Application1";

var activitySource = new ActivitySource(ApplicationName);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
.ConfigureResource(resource => resource.AddService(
        serviceName: ApplicationName))
    .WithTracing(tracing => tracing
        .AddEntityFrameworkCoreInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        // Add custom source
        .AddSource(ApplicationName)
        // .AddConsoleExporter()    
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://104.43.109.254:4317/v1/traces");
            options.Protocol = OtlpExportProtocol.Grpc;
        })).WithMetrics(metrics => metrics
            .AddMeter("Application1.Metrics")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://104.43.109.254:4317/v1/metrics");
                options.Protocol = OtlpExportProtocol.Grpc;
            })
            .AddAspNetCoreInstrumentation()
            .AddProcessInstrumentation()
            .AddRuntimeInstrumentation()
        )
    .WithLogging(loging => loging
    .AddOtlpExporter(otlpOptions => {

            otlpOptions.Protocol = OtlpExportProtocol.Grpc;
            otlpOptions.Endpoint = new Uri("http://104.43.109.254:4317/v1/logs");
                   
    }));


// builder.Services.AddSingleton<Instrumentation>();
builder.Services.AddSingleton(activitySource);

builder.Services.AddControllers(options =>
    {
        // options.Filters.Add<TraceActionFilter>();
    });

var app = builder.Build();
// app.UseMiddleware<TraceMiddleware>();

app.UseMiddleware<ConnectionMetricsMiddleware>();
app.MapControllers();


app.Run();
