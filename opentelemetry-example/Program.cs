using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var appName = "cal-service";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resouce => resouce
        .AddService(appName))
    .WithLogging(loging => loging
      .AddOtlpExporter(otlpOptions =>
      {
        otlpOptions.Protocol = OtlpExportProtocol.Grpc;
        otlpOptions.Endpoint = new Uri("http://otlp-endpoint/v1/logs");
      })
    )
    .WithTracing(trace => trace
      .AddAspNetCoreInstrumentation()
      .AddEntityFrameworkCoreInstrumentation()
      .AddHttpClientInstrumentation()
      // สำหรับการทดสอบเท่านั้น ไม่ควรใช้ใน Production เพราะจะทำให้เกิดการส่งข้อมูลที่ไม่จำเป็นไปยัง OTLP
      .AddConsoleExporter()
      .AddOtlpExporter(otlpOptions =>
        {
          otlpOptions.Protocol = OtlpExportProtocol.Grpc;
          otlpOptions.Endpoint = new Uri("http://otlp-endpoint/v1/traces");
        })
    )
    .WithMetrics(metrics => metrics
      .AddAspNetCoreInstrumentation()
      .AddHttpClientInstrumentation()
      .AddOtlpExporter(otlpOptions =>
      {
        otlpOptions.Protocol = OtlpExportProtocol.Grpc;
        otlpOptions.Endpoint = new Uri("http://otlp-endpoint/v1/metrics");
      })
    );

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
