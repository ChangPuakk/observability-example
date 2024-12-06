
using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var activitySource = new ActivitySource("Application2");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
.ConfigureResource(resource => resource.AddService(
        serviceName:  "Application2"))
    .WithTracing(tracing => tracing
        
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("Application2")
        // .AddConsoleExporter()    
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://alloy.internal.greenglacier-f8965d9a.southeastasia.azurecontainerapps.io:4317/v1/traces");
            options.Protocol = OtlpExportProtocol.Grpc;
        })).WithMetrics(metrics => metrics
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Application2"))
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://alloy.internal.greenglacier-f8965d9a.southeastasia.azurecontainerapps.io:4317/v1/metrics");
                options.Protocol = OtlpExportProtocol.Grpc;
            })
            .AddAspNetCoreInstrumentation()
            .AddProcessInstrumentation()
            .AddRuntimeInstrumentation()
            
        )
    .WithLogging(loging => loging
        .AddOtlpExporter(otlpOptions => {

                otlpOptions.Protocol = OtlpExportProtocol.Grpc;
                otlpOptions.Endpoint = new Uri("http://alloy.internal.greenglacier-f8965d9a.southeastasia.azurecontainerapps.io:4317/v1/logs");
                    
        }));


// builder.Services.AddSingleton<Instrumentation>();
builder.Services.AddSingleton(activitySource);
builder.Services.AddSingleton(new Meter("Application2.Metrics"));
builder.Services.AddControllers(options =>
    {
        // options.Filters.Add<TraceActionFilter>();
    });

var app = builder.Build();
// app.UseMiddleware<TraceMiddleware>();

app.MapControllers();



app.Run();
