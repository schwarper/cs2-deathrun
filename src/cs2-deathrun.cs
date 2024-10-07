using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using static CounterStrikeSharp.API.Core.Listeners;
using static Deathrun.Config_Config;
using static Deathrun.Terrorist;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Deathrun;

public class Deathrun : BasePlugin
{
    public override string ModuleName => "Deathrun";
    public override string ModuleVersion => "0.0.2";
    public override string ModuleAuthor => "schwarper";

    public Timer? RespawnTimer { get; set; }
    public bool AutoRestart = false;

    public override void Load(bool hotReload)
    {
        AddCommandListener("jointeam", Command_Jointeam);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
        RegisterListener<OnMapStart>(OnMapStart);

        Config_Config.Load();
    }

    public override void Unload(bool hotReload)
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
        RemoveListener<OnMapStart>(OnMapStart);
    }

    public HookResult Command_Jointeam(CCSPlayerController? player, CommandInfo info)
    {
        return HookResult.Handled;
    }

    public void OnMapStart(string mapname)
    {
        AutoRestart = true;
    }

    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (@event.Winner != 2 && Config.SlayCTIfLose)
        {
            List<CCSPlayerController> players = Utilities.GetPlayers().Where(p => p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive).ToList();

            foreach (CCSPlayerController? player in players)
            {
                player.CommitSuicide(false, true);
            }
        }

        Find(this);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundPrestart @event, GameEventInfo info)
    {
        if (!Check(this))
        {
            return HookResult.Continue;
        }

        if (Config.RespawnTime > 0)
        {
            RespawnTimer?.Kill();

            Server.ExecuteCommand("mp_respawn_on_death_ct true");

            RespawnTimer = AddTimer(Config.RespawnTime, () =>
            {
                Server.PrintToChatAll(Config.Tag + Localizer["Respawn time is over"]);
                Server.ExecuteCommand("mp_respawn_on_death_ct false");
            });
        }
        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Pre)]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (@event.Userid is not CCSPlayerController player)
        {
            return HookResult.Continue;
        }

        switch (player.Team)
        {
            case CsTeam.Terrorist:
                {
                    player.ChangeTeam(CsTeam.CounterTerrorist);
                    break;
                }
            case CsTeam.CounterTerrorist:
                {
                    var attacker = @event.Attacker;

                    if (Config.GiveFragWhenCTDies && (@event.Attacker == null || attacker == player))
                    {
                        attacker = GetRandomTerrorist();

                        if (attacker == null)
                        {
                            return HookResult.Continue;
                        }

                        attacker.ActionTrackingServices!.MatchStats.Kills += 1;

                        Utilities.SetStateChanged(player, "CCSPlayerController", "m_pActionTrackingServices");
                        Utilities.SetStateChanged(player, "CCSPlayerController", "m_iScore");

                        @event.Weapon = "planted_c4";
                        @event.Attacker = attacker;
                        return HookResult.Changed;
                    }
                    break;
                }
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event.Userid is not CCSPlayerController player)
        {
            return HookResult.Continue;
        }

        if (TerroristsList.Contains(player))
        {
            return HookResult.Continue;
        }

        TerroristsList.Remove(player);

        if (Config.SaveTerroristDisconnect)
        {
            FugitiveTerroristsList.Add(player);
        }

        AddTimer(1.0f, () =>
        {
            var playersCount = Utilities.GetPlayers().Count;

            if (playersCount == 0)
            {
                OnMapStart(string.Empty);
            }
        });

        Server.PrintToChatAll(Config.Tag + Localizer["T Left game", @event.Name]);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event.Userid is not CCSPlayerController player)
        {
            return HookResult.Continue;
        }

        AddTimer(1.0f, () =>
        {
            if (player.IsValid)
            {
                player.ChangeTeam(CsTeam.CounterTerrorist);
            }

            if (AutoRestart)
            {
                Server.ExecuteCommand("mp_restartgame 5");
                AutoRestart = false;
            }
        });

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        if (@event.Userid is not CCSPlayerController player)
        {
            return HookResult.Continue;
        }

        if (!TerroristsList.Contains(player))
        {
            return HookResult.Continue;
        }

        AddTimer(1.0f, () =>
        {
            player.PlayerPawn.Value!.VelocityModifier = Config.Speed;
        });

        return HookResult.Continue;
    }

    public HookResult OnTakeDamage(DynamicHook hook)
    {
        CEntityInstance entity = hook.GetParam<CEntityInstance>(0);

        if (entity.DesignerName != "player")
        {
            return HookResult.Continue;
        }

        var info = hook.GetParam<CTakeDamageInfo>(1);

        if (Config.NoFallDamage && info.BitsDamageType == DamageTypes_t.DMG_FALL)
        {
            var player = new CCSPlayerPawn(entity.Handle).OriginalController.Value;

            if (player != null && player.Team == CsTeam.Terrorist)
            {
                return HookResult.Handled;
            }
        }

        return HookResult.Continue;
    }
}