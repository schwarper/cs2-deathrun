using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static Deathrun.Deathrun;

namespace Deathrun;

public static class Terrorist
{
    private static Random Random { get; set; } = new();

    public static bool Find(bool minLog)
    {
        if (PlayerTerrorist != null)
        {
            return false;
        }

        List<CCSPlayerController> allPlayers = Utilities.GetPlayers();

        if (allPlayers.Count < Instance.Config.MinPlayers)
        {
            if (minLog)
            {
                Server.PrintToChatAll(Instance.Config.Tag + Instance.Localizer["Minimum player", Instance.Config.MinPlayers]);
            }

            return false;
        }

        PlayerTerrorist = allPlayers[Random.Next(allPlayers.Count)];
        PlayerTerrorist.SwitchTeam(CsTeam.Terrorist);
        Server.PrintToChatAll(Instance.Config.Tag + Instance.Localizer["Is terrorist", PlayerTerrorist.PlayerName]);
        return true;
    }
}