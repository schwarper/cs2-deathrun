using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using static Deathrun.Deathrun;

namespace Deathrun;

public static class Event
{
    private static bool AutoRestart { get; set; }

    public static void Load()
    {
        Instance.AddCommandListener("jointeam", Command_Jointeam, HookMode.Pre);

        Instance.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        Instance.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        Instance.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        Instance.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        Instance.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        Instance.RegisterEventHandler<EventWarmupEnd>(OnWarmupEnd);
    }

    public static void Unload()
    {
        Instance.RemoveCommandListener("jointeam", Command_Jointeam, HookMode.Pre);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
    }

    public static HookResult Command_Jointeam(CCSPlayerController? player, CommandInfo commandInfo)
    {
        return HookResult.Handled;
    }

    public static HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        if (!AutoRestart)
        {
            AutoRestart = true;
            Server.ExecuteCommand("mp_restartgame 3");
        }

        if (PlayerTerrorist == null)
        {
            if (!Terrorist.Find())
            {
                return HookResult.Continue;
            }
        }

        CCSPlayerController? player = @event.Userid;

        if (player == null || player.Team != CsTeam.Terrorist)
        {
            return HookResult.Continue;
        }

        CCSPlayerPawn? playerPawn = player.PlayerPawn.Value;

        if (playerPawn == null)
        {
            return HookResult.Continue;
        }

        playerPawn.VelocityModifier = Instance.Config.TSpeed;

        return HookResult.Continue;
    }

    public static HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        List<CCSPlayerController> allPlayers = Utilities.GetPlayers();

        if (@event.Winner == 2 && Instance.Config.SlayCT)
        {
            List<CCSPlayerController> ctAlivePlayers = allPlayers.Where(player => player.Team == CsTeam.CounterTerrorist && player.PawnIsAlive).ToList();

            foreach (CCSPlayerController? player in ctAlivePlayers)
            {
                player.CommitSuicide(false, true);
            }

            Server.PrintToChatAll(Instance.Config.Tag + Instance.Localizer["CT Lost"]);
        }

        PlayerTerrorist?.SwitchTeam(CsTeam.CounterTerrorist);

        foreach (CCSPlayerController player in allPlayers)
        {
            if (!player.PawnIsAlive)
            {
                player.TakesDamage = false;
            }
        }

        Terrorist.Find();

        return HookResult.Continue;
    }

    public static HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        if (player != PlayerTerrorist)
        {
            return HookResult.Continue;
        }

        Server.PrintToChatAll(Instance.Config.Tag + Instance.Localizer["T Left"]);

        Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules?.TerminateRound(1.0f, RoundEndReason.RoundDraw);

        return HookResult.Continue;
    }

    public static HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        Instance.AddTimer(0.1f, () => player.ChangeTeam(CsTeam.CounterTerrorist));

        return HookResult.Continue;
    }

    public static HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        if (player != PlayerTerrorist)
        {
            return HookResult.Continue;
        }

        Server.PrintToChatAll(Instance.Config.Tag + Instance.Localizer["T Left"]);

        Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules?.TerminateRound(1.0f, RoundEndReason.RoundDraw);

        return HookResult.Continue;
    }

    public static HookResult OnWarmupEnd(EventWarmupEnd @event, GameEventInfo info)
    {
        Terrorist.Find();

        return HookResult.Continue;
    }

    public static HookResult OnTakeDamage(DynamicHook hook)
    {
        if (Instance.Config.TKills == 0 || PlayerTerrorist == null)
        {
            return HookResult.Continue;
        }

        CCSPlayerPawn entity = hook.GetParam<CCSPlayerPawn>(0);

        if (entity.DesignerName != "player")
        {
            return HookResult.Continue;
        }

        CCSPlayerController victim = entity.Controller.Value!.As<CCSPlayerController>();

        if (victim.Team != CsTeam.CounterTerrorist)
        {
            return HookResult.Continue;
        }

        CTakeDamageInfo? info = hook.GetParam<CTakeDamageInfo>(1);

        if (info.Attacker.IsValid)
        {
            return HookResult.Continue;
        }

        if (info.Damage < victim.Health)
        {
            return HookResult.Continue;
        }

        KillInfo.Create(victim, PlayerTerrorist);
        hook.SetReturn(false);

        return HookResult.Continue;
    }
}