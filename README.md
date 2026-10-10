# Better Casino

A MelonLoader mod for **Schedule I** that overhauls the casino: bigger bets, configurable slot odds, a casino that never closes, and a safe place to lose the cops.

[Download on Nexus Mods](https://www.nexusmods.com/schedule1/mods/1312) · Version 4.0.0

## Features

### Slot Machines
- **Bet ladder.** The game's own $5, $10, $25, $50, $100 continued in the same pattern up to $100k (250, 500, 1k, 2.5k, 5k ... 100k), trimmed to your own minimum and maximum.
- **Compact labels.** Large bets show as `$2k`, `$100k` and auto-fit the machine display.
- **Free play.** Optional `$0` bet for practice spins.
- **Real win chance.** Set the percent chance that a spin pays out. Wins still use the game's own payout tiers (Jackpot 100×, Bells 25×, matching fruit 10×, mixed fruit 2×).
- **Win penalties.** One switch turns on optional penalties for hopping between machines and for long win streaks.

### Table Games
- **Custom bet limits** for Blackjack and Ride the Bus (default $10 – $5,000, the game caps at $1,000).
- **Smart slider.** Bets snap to round amounts so you can actually land on $2,500 instead of $2,487.

### Always Open
- The casino stays open 24/7. Only the casino is affected. The Dark Market and other timed areas keep their normal hours.
- The `4PM-5AM` sign is replaced with your own text (default `OPEN 24/7`) and restored if you turn it off.

### Casino Safehouse
- Your wanted level is cleared while you are inside the casino, with the same result as escaping the cops normally.

## Settings

Every setting can be changed in-game with [Mod Manager & Phone App](https://www.nexusmods.com/schedule1/mods/397), where each one has a description and changes apply instantly. Without it, edit `UserData/MelonPreferences.cfg`.

Settings from 3.x are migrated automatically.

#### `[BetterCasino]` General
| Setting | Default | Description |
|---|---|---|
| `AlwaysOpen` | `true` | Keep the casino open around the clock |
| `CasinoSafehouse` | `true` | Clear your wanted level inside the casino |
| `SignChangerEnabled` | `true` | Replace the opening hours on the casino sign |
| `SignText` | `OPEN 24/7` | Text shown on the sign |
| `SlotMachineEnabled` | `true` | Enable slot machine tweaks |
| `TableGamesEnabled` | `true` | Enable table game tweaks |

#### `[BetterCasino_Slots]` Slot Machines
| Setting | Default | Description |
|---|---|---|
| `FreePlay` | `false` | Add a `$0` bet (free spins never pay out) |
| `LowerLimit` | `5` | Smallest bet on the ladder |
| `UpperLimit` | `100000` | Largest bet on the ladder |

#### `[BetterCasino_SlotsOdds]` Slot Machine Odds
| Setting | Default | Description |
|---|---|---|
| `ChancePercent` | `-1` | Percent chance (0 – 100) that a spin pays out. `-1` keeps the game's odds (about 31%) |
| `PenaltiesEnabled` | `false` | Turns on the hopping and streak penalties below |
| `PenaltyHopPercent` | `50` | Win chance is cut by this percent right after switching machines |
| `PenaltyHopSeconds` | `10` | Seconds the hopping penalty lasts |
| `PenaltyStreakPercent` | `10` | Win chance drops by this percent for every win past the threshold |
| `PenaltyStreakThreshold` | `3` | Wins in a row before the streak penalty starts |

The odds settings only apply when `ChancePercent` is not `-1`, and the penalties only when `PenaltiesEnabled` is on.

#### `[BetterCasino_Tables]` Table Games
| Setting | Default | Description |
|---|---|---|
| `BlackjackLowerLimit` | `10` | Blackjack minimum bet |
| `BlackjackUpperLimit` | `5000` | Blackjack maximum bet |
| `RideTheBusLowerLimit` | `10` | Ride the Bus minimum bet |
| `RideTheBusUpperLimit` | `5000` | Ride the Bus maximum bet |

## Multiplayer

Every feature runs on the player who has the mod. The slot win chance applies to the spins you start, so it works for clients as well as the host. Everyone at a slot machine should use the same slot settings. The machine syncs its bet *position*, so different ladders would show different amounts.

## Installation

1. Install [MelonLoader](https://melonwiki.xyz/) (0.7 or newer)
2. Drop `BetterCasino.dll` into `Schedule I/Mods`
3. Launch the game once to generate the config
4. *(Optional)* Install [Mod Manager & Phone App](https://www.nexusmods.com/schedule1/mods/397) for in-game settings

## Building

The project references MelonLoader and game assemblies from folders next to the repo, which are not included:

```
Desktop/
├── Better Casino/          (this repo)
├── Libs/
│   ├── net6/               ← <GameDir>/MelonLoader/net6/
│   └── Il2CppAssemblies/   ← <GameDir>/MelonLoader/Il2CppAssemblies/
└── ModManager/
    └── ModManager&PhoneApp.dll
```

Run the game once with MelonLoader so it generates `Il2CppAssemblies`, then:

```
dotnet build BetterCasino.csproj -c Release
```

Output: `bin/Release/net6.0/BetterCasino.dll`

## Compatibility

- MelonLoader 0.7+ (IL2CPP)
- May conflict with other mods that change `SlotMachine.BetAmounts`, the casino bet panel, or `TimedAccessZone`

## Credits

Created by **Zyhloh**. Thanks to BorisJohnsonLies for the sign patch and free play ideas.
