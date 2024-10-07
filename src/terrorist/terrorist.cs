using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static Deathrun.Config_Config;

namespace Deathrun;

public static class Terrorist
{
    public static readonly HashSet<CCSPlayerController> TerroristsList = [];
    public static readonly HashSet<CCSPlayerController> FugitiveTerroristsList = [];
    public static readonly Random random = new();

    public static CCSPlayerController? GetRandomTerrorist()
    {
        if (TerroristsList.Count == 0)
        {
            return null;
        }

        int index = random.Next(TerroristsList.Count);
        return TerroristsList.ElementAt(index);
    }
    public static void Find(Deathrun Instance)
    {
        TerroristsList.Clear();

        List<CCSPlayerController> players = Utilities.GetPlayers();
        int playerCount = players.Count;

        if (playerCount < Config.TerroristCount.Keys.Min())
        {
            return;
        }

        int terroristsNeeded = Config.TerroristCount
            .Where(kvp => playerCount >= kvp.Key)
            .OrderByDescending(kvp => kvp.Key)
            .FirstOrDefault()
            .Value;

        List<CCSPlayerController> selectedPlayers = Config.SaveTerroristDisconnect
            ? FugitiveTerroristsList.Take(terroristsNeeded).ToList()
            : [];

        TerroristsList.UnionWith(selectedPlayers);
        FugitiveTerroristsList.ExceptWith(selectedPlayers);

        int remainingTerroristsNeeded = terroristsNeeded - selectedPlayers.Count;
        if (remainingTerroristsNeeded > 0)
        {
            selectedPlayers = players.Except(TerroristsList).OrderBy(_ => random.Next()).Take(remainingTerroristsNeeded).ToList();
            TerroristsList.UnionWith(selectedPlayers);
        }

        if (TerroristsList.Count != 0)
        {
            string terroristNames = string.Join(", ", TerroristsList.Select(p => p.PlayerName));

            if (TerroristsList.Count == 1)
            {
                Server.PrintToChatAll(Config.Tag + Instance.Localizer["Terrorist is", terroristNames]);
            }
            else
            {
                Server.PrintToChatAll(Config.Tag + Instance.Localizer["Terrorists are", terroristNames]);
            }
        }
    }

    public static bool Check(Deathrun Instance)
    {
        TerroristsList.RemoveWhere(player => !player.IsValid);

        if (!Config.SaveTerroristDisconnect)
        {
            FugitiveTerroristsList.Clear();
        }

        var players = Utilities.GetPlayers().Except(TerroristsList).ToList();

        foreach (var player in players)
        {
            player.SwitchTeam(CsTeam.CounterTerrorist);
        }

        foreach (CCSPlayerController player in TerroristsList)
        {
            player.SwitchTeam(CsTeam.Terrorist);
        }

        int playerCount = Utilities.GetPlayers().Count;
        int terroristsNeeded = Config.TerroristCount
            .Where(kvp => playerCount >= kvp.Key)
            .OrderByDescending(kvp => kvp.Key)
            .FirstOrDefault()
            .Value;

        if (playerCount < Config.TerroristCount.Keys.Min())
        {
            Server.PrintToChatAll(Config.Tag + Instance.Localizer["Not enough player"]);
            return false;
        }

        if (TerroristsList.Count < terroristsNeeded)
        {
            while (TerroristsList.Count < terroristsNeeded)
            {
                List<CCSPlayerController> remainingPlayers = Config.SaveTerroristDisconnect
                    ? FugitiveTerroristsList.Where(p => p.IsValid).ToList()
                    : [];

                if (remainingPlayers.Count == 0)
                {
                    remainingPlayers = Utilities.GetPlayers().Except(TerroristsList).Where(p => p.IsValid).ToList();
                }

                if (remainingPlayers.Count == 0)
                {
                    Server.PrintToChatAll(Config.Tag + Instance.Localizer["Not enough player"]);
                    break;
                }

                CCSPlayerController newTerrorist = remainingPlayers.OrderBy(_ => random.Next()).First();
                newTerrorist.SwitchTeam(CsTeam.Terrorist);
                TerroristsList.Add(newTerrorist);
                FugitiveTerroristsList.Remove(newTerrorist);
            }

            if (TerroristsList.Count >= terroristsNeeded)
            {
                string terroristNames = string.Join(", ", TerroristsList.Select(p => p.PlayerName));

                if (TerroristsList.Count == 1)
                {
                    Server.PrintToChatAll(Config.Tag + Instance.Localizer["Terrorist is", terroristNames]);
                }
                else
                {
                    Server.PrintToChatAll(Config.Tag + Instance.Localizer["Terrorists are", terroristNames]);
                }
            }
        }

        return true;
    }
}