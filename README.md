
# cs2-deathrun

If you want to donate or need a help about plugin, you can contact me in discord private/server

Discord nickname: schwarper

Discord link : [Discord server](https://discord.gg/4zQfUzjk36)

## Configuration
```toml
[Tag]
Tag = "{red}[Deathrun] "

[Deathrun]
TerroristCount = { 2 = 1, 16 = 2, 26 = 3 } # Sets the number of terrorists.
SaveTerroristDisconnect = true # Detects a player who leaves the game as a terrorist. Makes him a terrorist when he enters the game.

[Terrorist]
Speed = 2.0
GiveFragWhenCTDies = true
NoFallDamage = true

[CounterTerrorist]
SlayCTIfLose = true
RespawnTime = 30.0
```
