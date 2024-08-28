# Observability Example

Steps

- [Observability Example](#observability-example)
  - [Create application project](#create-application-project)
  - [Install OpenTelemtry](#install-opentelemtry)
  - [Setup OpenTelemetry to application](#setup-opentelemetry-to-application)

## Create application project

สร้างโปรเจคใหม่โดยใช้คำสั่ง

```ssh
dotnet new web
```

สร้างไฟล์ `Controllers/CalculateController.cs`

```cs
using Microsoft.AspNetCore.Mvc;
using System.Net;

public class CalculateController : ControllerBase
{

  public CalculateController()
  {}

  [HttpGet("sum")]
  public int Sum(List<int> numbers)
  {
    var sum = numbers.Sum();
    
    return sum;
  }
}
```

แก้ไขไฟล์ `Program.cs`

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();

```

ตั้งค่า endpoint ใน `Properties/launchSettings.js`

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:8080",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

จากนั้นทดสอบรันโปรเจคโดยใช้คำสั่ง

```ssh
dotnet run
```

โดยเปิด Browser เข้าไปที่ `http://localhost:8080/sum?numbers=1&numbers=2&numbers=3`

## Install OpenTelemtry

ใช้คำสั่งด้านล่างเพื่อติดตั้ง OpenTelemetry ลงในโปรเจค

```ssh
dotnet add package OpenTelemetry
dotnet add package OpenTelemetry.Exporter.Console
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
```

## Setup OpenTelemetry to application

Configure Exporter และ Instrumentation ใน `Program.cs`

```cs
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resouce => resouce
        .AddService(appName))
    .WithLogging(loging => loging
      // Export ไปยัง Datasource
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

```

สำหรับ logger จะต้องเพิ่ม Dependency Injection เข้าไปใน Controller

Update ``Controllers/CalculateController.cs``

```cs
using Microsoft.AspNetCore.Mvc;
using System.Net;

public class CalculateController : ControllerBase
{

  private ILogger<CalculateController> _logger;

  public CalculateController(ILogger<CalculateController> logger)
  {
    _logger = logger;
  }

  [HttpGet("sum")]
  public int Sum(List<int> numbers)
  {
    var sum = numbers.Sum();
    _logger.LogInformation($"Sum of {string.Join(", ", numbers)} is {sum}");
    return sum;
  }
}
```

จากนั้นใช้คำสั่ง `dotnet run` เพื่อรันโปรแกรม

เปิด `http://localhost:8080/sum?numbers=1&numbers=2&numbers=3` เพื่อทดสอบ

Tracing จะแสดงผลใน Console

```
Activity.TraceId:            a9e96a12d58fca891c7be54b4fd5b6fb
Activity.SpanId:             20097573c4ed58d2
Activity.TraceFlags:         Recorded
Activity.ActivitySourceName: Microsoft.AspNetCore
Activity.DisplayName:        GET sum
Activity.Kind:               Server
Activity.StartTime:          2024-08-28T04:01:16.6275660Z
Activity.Duration:           00:00:00.0836277
Activity.Tags:
    server.address: localhost
    server.port: 8080
    url.query: ?numbers=Redacted&numbers=Redacted&numbers=Redacted
    http.request.method: GET
    url.scheme: http
    url.path: /sum
    network.protocol.version: 1.1
    user_agent.original: Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36
    http.route: sum
    http.response.status_code: 200
Resource associated with Activity:
    service.name: cal-service
    service.instance.id: 9680c430-1f7b-48d9-ab9a-70c511beccee
    telemetry.sdk.name: opentelemetry
    telemetry.sdk.language: dotnet
    telemetry.sdk.version: 1.9.0

Activity.TraceId:            ac52ce6db754b0c8a6f3804394fe641e
Activity.SpanId:             252f060983cd977e
Activity.TraceFlags:         Recorded
Activity.ActivitySourceName: Microsoft.AspNetCore
Activity.DisplayName:        GET
Activity.Kind:               Server
Activity.StartTime:          2024-08-28T04:01:16.7718958Z
Activity.Duration:           00:00:00.0025821
Activity.Tags:
    server.address: localhost
    server.port: 8080
    http.request.method: GET
    url.scheme: http
    url.path: /favicon.ico
    network.protocol.version: 1.1
    user_agent.original: Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36
    http.response.status_code: 404
Resource associated with Activity:
    service.name: cal-service
    service.instance.id: 9680c430-1f7b-48d9-ab9a-70c511beccee
    telemetry.sdk.name: opentelemetry
    telemetry.sdk.language: dotnet
    telemetry.sdk.version: 1.9.0
```

[Example](/opentelemetry-example/Program.cs)

Next Topic

[Custom ActivitySource Tracing](/custom-activitysource-tracing/README.md)