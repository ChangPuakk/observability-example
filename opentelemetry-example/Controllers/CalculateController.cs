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