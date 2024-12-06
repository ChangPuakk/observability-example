/*Dice.cs*/
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DiceSystem.Models;



public class Dice
{
    private int min;
    private int max;
    public ActivitySource activitySource;

    private static Meter meter = new Meter("Application1.Metrics");
    private Counter<long> connectionCounter = meter.CreateCounter<long>("dice", "roll");

    public Dice(int min, int max, ActivitySource activitySource)
    {
        this.min = min;
        this.max = max;
        this.activitySource = activitySource;
    }

  
    public List<int> rollTheDice(int rolls)
    {
        List<int> results = new List<int>();
        // using (var myActivity = activitySource.StartActivity("rollTheDice"))
        // {
          for (int i = 0; i < rolls; i++)
          {
              results.Add(rollOnce());
          }

           return results;
        // }
    }
    [Trace("rollOnce")]
    private int rollOnce()
    {
        using (var childActivity = activitySource.StartActivity("rollOnce"))
        {
          int result;

          result = Random.Shared.Next(min, max + 1);
        //   childActivity?.SetTag("dicelib.rolled", result);
          this.connectionCounter.Add(1);
          return result;
        }
    }
}