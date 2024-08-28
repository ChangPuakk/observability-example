# Custom ActivitySource Tracing

Table of contents

- [Custom ActivitySource Tracing](#custom-activitysource-tracing)
  - [Configure OpenTelemetry](#configure-opentelemetry)
  - [Update Controller](#update-controller)
  - [Add Custom Tracing](#add-custom-tracing)

## Configure OpenTelemetry

Update `Program.cs`

```cs
builder.Services.AddOpenTelemetry()

  ...

  .WithTracing(trace => trace
      
      ...
      
      // Add custom source to the global tracer
      .AddSource(appName)
      
      ...

    );


// Add custom source to the global tracer
builder.Services.AddSingleton(new ActivitySource(appName));

var app = builder.Build();


app.MapControllers();

app.Run();

```

## Update Controller

เพิ่ม Dependencies Injection ใน controller

`Controllers/DiceController.cs`

```cs
using DiceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;

namespace DiceSystem.Controllers;

public class DiceController : ControllerBase
{
    private ILogger<DiceController> logger;
    private ActivitySource _activitySource;

    public DiceController(ILogger<DiceController> logger, ActivitySource activitySource)
    {
        this.logger = logger;
        this._activitySource = activitySource;
    }

    [HttpGet("/rolldice")]
    public List<int> RollDice(string player, int? rolls)
    {
        if(!rolls.HasValue)
        {
            logger.LogError("Missing rolls parameter");
            throw new HttpRequestException("Missing rolls parameter", null, HttpStatusCode.BadRequest);
        }

        var result = new Dice(1, 6, _activitySource).rollTheDice(rolls.Value);

        if (string.IsNullOrEmpty(player))
        {
            logger.LogInformation("Anonymous player is rolling the dice: {result}", result);
        }
        else
        {
            logger.LogInformation("{player} is rolling the dice: {result}", player, result);
        }

        return result;
    }
}
```

## Add Custom Tracing

เพิ่ม Scope StartActivity ในส่วนที่ต้องการ

```cs
  using var activity = _activitySource.StartActivity("activity1"))
  {
    ...
  }
```

Update `Models/Dice.cs`

```cs
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DiceSystem.Models;



public class Dice
{
    private int min;
    private int max;
    private ActivitySource _activitySource;

    public Dice(int min, int max, ActivitySource activitySource)
    {
        this.min = min;
        this.max = max;
        _activitySource = activitySource;
    }

  
    public List<int> rollTheDice(int rolls)
    {
        List<int> results = new List<int>();
        using (var myActivity = _activitySource.StartActivity("rollTheDice"))
        {
          // Set the tag to the number of rolls
          myActivity?.SetTag("rolls.times", rolls);
          for (int i = 0; i < rolls; i++)
          {
              results.Add(rollOnce());
          }

           return results;
        }
    }
    private int rollOnce()
    {
        using (var childActivity = _activitySource.StartActivity("rollOnce"))
        {
          int result;

          result = Random.Shared.Next(min, max + 1);
          
          return result;
        }
    }
}
```