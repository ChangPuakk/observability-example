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