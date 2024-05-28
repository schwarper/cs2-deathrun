using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;

namespace Deathrun;

public class Deathrun : BasePlugin, IPluginConfig<DeathrunConfig>
{
    public override string ModuleName => "Deathrun Mod";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "schwarper";

    public DeathrunConfig Config { get; set; } = new DeathrunConfig();
    public static Deathrun Instance { get; set; } = new();
    public static CCSPlayerController? PlayerTerrorist { get; set; }

    public override void Load(bool hotReload)
    {
        Instance = this;

        if (hotReload)
        {
            Server.ExecuteCommand("mp_restartgame 1");
        }

        Event.Load();
    }

    public override void Unload(bool hotReload)
    {
        Event.Unload();
    }

    public void OnConfigParsed(DeathrunConfig config)
    {
        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);

        Config = config;
    }
}