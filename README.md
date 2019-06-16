**Rogue Lite** is a small gamemode that kicks and prevents players rejoining the server if they have expended the configurable amount of lives each player gets.

## Commands

### Admin

* `setmaxlives [#]` -- Sets the amount of allowed lives for each player.
* `cleardeaths` -- Clears out the death counts for all players.

### Console

* `cleardeaths` -- Clears out the death counts for all players.

## Configuration

* "MaxLives": 3 -- You are able to set the amount of lives each player gets.
* "ClearDeathsAfterWipe": true -- If true the plugin will automatically clear the death counts if a new wipe is detected.
* "PlaySoundOnPlayerKick": true -- If true the plugin will play a global soundeffect when someone expends their last life.
* "SoundEffect": "assets/prefabs/tools/medical syringe/effects/pop_cap.prefab" -- The specified soundeffect.
```json
{
  "Options": {
    "MaxLives": 3,
    "ClearDeathsAfterWipe": true,
    "PlaySoundOnPlayerKick": true,
    "SoundEffect": "assets/prefabs/tools/medical syringe/effects/pop_cap.prefab"
  }
}
```

## Localization

### English

```json
{
  "PlayerKicked": "<color=red>{0}</color> has expended their last life and has been kicked from the server!",
  "KickReason": "☠ YOU DIED ☠",
  "KickWarning": "<color=red>☠</color>\nYou have {0} lives(s) remaining!",
  "SetMaxLives": "MaxLives on the server set to <color=yellow>{0}</color>!",
  "ClearDeaths": "Deaths cleared."
}
```

## Credits

### Beta Testers

* Reaper2972
* CMDRWrecks
* Momo
* Barron
* Doctor_MooDM
* Mr.TradeAHoe
* luckyxjake
* Toxik Penut
* HakuRin
