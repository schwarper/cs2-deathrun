using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core.Translations;
using System.Reflection;
using Tomlyn;
using Tomlyn.Model;

namespace Deathrun;

public static class Config_Config
{
    public static Cfg Config { get; set; } = new Cfg();

    public static void Load()
    {
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;
        string configPath = Path.Combine(Server.GameDirectory,
                "csgo",
                "addons",
                "counterstrikesharp",
                "configs",
                "plugins",
                assemblyName,
                "config.toml"
            );

        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"Configuration file not found: {configPath}");
        }

        string configText = File.ReadAllText(configPath);
        TomlTable model = Toml.ToModel(configText);

        TomlTable tagTable = (TomlTable)model["Tag"];
        Config.Tag = StringExtensions.ReplaceColorTags(tagTable["Tag"].ToString()!);

        TomlTable stTable = (TomlTable)model["Deathrun"];
        Config.TerroristCount = [];
        foreach (KeyValuePair<string, object> kvp in (TomlTable)stTable["TerroristCount"])
        {
            Config.TerroristCount[int.Parse(kvp.Key)] = int.Parse(kvp.Value.ToString()!);
        }
        Config.SaveTerroristDisconnect = bool.Parse(stTable["SaveTerroristDisconnect"].ToString()!);

        TomlTable tTable = (TomlTable)model["Terrorist"];
        Config.Speed = float.Parse(tTable["Speed"].ToString()!);
        Config.GiveFragWhenCTDies = bool.Parse(tTable["GiveFragWhenCTDies"].ToString()!);
        Config.NoFallDamage = bool.Parse(tTable["NoFallDamage"].ToString()!);

        TomlTable ctTable = (TomlTable)model["CounterTerrorist"];
        Config.SlayCTIfLose = bool.Parse(ctTable["SlayCTIfLose"].ToString()!);
        Config.RespawnTime = float.Parse(ctTable["RespawnTime"].ToString()!);
    }

    public class Cfg
    {
        public string Tag { get; set; } = string.Empty;
        public Dictionary<int, int> TerroristCount { get; set; } = [];
        public bool SaveTerroristDisconnect { get; set; }
        public float Speed { get; set; }
        public bool GiveFragWhenCTDies { get; set; }
        public bool NoFallDamage { get; set; }
        public bool SlayCTIfLose { get; set; }
        public float RespawnTime { get; set; }
    }
}