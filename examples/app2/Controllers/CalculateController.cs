
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace App1.Controllers;

    public class CalculateController : ControllerBase
    {
        private ILogger<CalculateController> logger;
        private ActivitySource _activitySource;

        public CalculateController(ILogger<CalculateController> logger, ActivitySource activitySource)
        {
            this.logger = logger;
            _activitySource = activitySource;
        }

        [HttpGet("sum")]
        // [Trace("Sum")]
        public int Sum(List<int> numbers)
        {
        //   using var activity = _activitySource.StartActivity("Sum");
          var sum = numbers.Sum();
          this.logger.LogInformation($"Sum of {string.Join(", ", numbers)} is {sum}");
          return sum;
        }
    }
