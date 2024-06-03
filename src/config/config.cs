using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace Deathrun;

public class DeathrunConfig : BasePluginConfig
{
    [JsonPropertyName("dr_tag")] public string Tag { get; set; } = "{red}[Deathrun] ";
    [JsonPropertyName("dr_min_players")] public int MinPlayers { get; set; } = 2;
    [JsonPropertyName("dr_tr_t_speed")] public float TSpeed { get; set; } = 1.0f;
    [JsonPropertyName("dr_slay_ct")] public bool SlayCT { get; set; } = true;
    [JsonPropertyName("dr_ct_respawn_time")] public float RespawnCT { get; set; } = 5.0f;
}