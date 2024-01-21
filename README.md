# Bannerlord_BattleLink

## Description

Engage in epic battles with or against your friends to determine the fate of the single-player campaign in Mount & Blade II: Bannerlord.

## Installation

### Server

Disclaimer: Proceed with caution as this delivery comes with no warranty for security issues.

#### Maps

Copy all maps from:
Mount & Blade II Bannerlord\Modules\SandBoxCore\SceneObj
to:
Mount & Blade II Dedicated Server\Modules\SandBoxCoreMP\SceneObj

#### Secret

Set the secret (36 charactes long min) in file:
Mount & Blade II Dedicated Server\Modules\BattleLink\config.properties

You can generate via PowerShell (Shift + Right-click):
```bash
[guid]::NewGuid()
```

### Singleplayer

In file:
Mount & Blade II Dedicated Server\Modules\BattleLink.Singleplayer\config.properties

Set the secret (minimum 36 characters long) (same as server).
Set IP of your server.

In file:
Mount & Blade II Dedicated Server\Modules\BattleLink.Singleplayer\player.txt
Set you multiplayer username (to reserve character in multiplayer)

#### Synchro Manually

You can also synchronize manually:
Copy paste your initializer file 
from:
Mount & Blade II Bannerlord\Modules\BattleLink.Singleplayer\Battles\BL_MPBattle_XXXX_Initializer.xml
to:
\Mount & Blade II Dedicated Server\Modules\BattleLink\Battles\Pending\


Copy paste your result file:
from:
\Mount & Blade II Dedicated Server\Modules\BattleLink\Battles\Finished\BL_MPBattle_XXXX_Result.csbin
to:
Mount & Blade II Bannerlord\Modules\BattleLink.Singleplayer\Battles\

#### Maps

Copy all maps from:
Mount & Blade II Bannerlord\Modules\SandBoxCore\SceneObj
to:
Mount & Blade II Bannerlord\Modules\SandBoxCoreMP\SceneObj

## Work In Progress

This project is a work in progress and lacks many features. 

Currently, it only supports battles in open fields.

Tasklist:
AI debug
Armor calculations of singleplayer
Perks of singleplayer
2 Teams vs 2 Teams
Bannerer
Siege Battle
...

## Credits

The base of the team select view was inspired by https://github.com/Byak0/Alliance

## Source

The source code is available at:
https://github.com/Metherlance/Bannerlord_BattleLink
It requires cleaning up but can be accessed for reference.

## License

You are free to modify and reuse this mod under the MIT License.
[MIT](https://choosealicense.com/licenses/mit/)

## Donation

If you've been eagerly awaiting this mod and enjoy it, consider buying me a coffee as a token of appreciation:
https://ko-fi.com/metherlance
(Note: This is not for continued development of the mod.)
