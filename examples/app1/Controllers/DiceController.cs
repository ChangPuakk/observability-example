using DiceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;

namespace DiceSystem.Controllers;

public class DiceController : ControllerBase
{
    private ILogger<DiceController> logger;
    private ActivitySource _activitySource;

    private HttpClient _httpClient = new HttpClient();

    public DiceController(ILogger<DiceController> logger, ActivitySource activitySource)
    {
        this.logger = logger;
        this._activitySource = activitySource;
    }

    [HttpGet("/rolldice")]
    // [Trace("RollDice")]
    public async Task<List<int>> RollDiceAsync(string player, int? rolls)
    {
        if(!rolls.HasValue)
        {
            logger.LogError("Missing rolls parameter");
            throw new HttpRequestException("Missing rolls parameter", null, HttpStatusCode.BadRequest);
        }

        var result = new Dice(1, 6, this._activitySource).rollTheDice(rolls.Value);

        if (string.IsNullOrEmpty(player))
        {
            logger.LogInformation("Anonymous player is rolling the dice: {result}", result);
        }
        else
        {
            logger.LogInformation("{player} is rolling the dice: {result}", player, result);
        }
         var response = await _httpClient.GetAsync("http://localhost:5180/sum?numbers=1&numbers=2&numbers=3");
                        // response.EnsureSuccessStatusCode();
                        // var result1 = await response.Content.ReadAsStringAsync();
        return result;
    }
}