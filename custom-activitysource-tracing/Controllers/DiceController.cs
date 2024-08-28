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