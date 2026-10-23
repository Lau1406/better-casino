using System;
using System.Collections.Generic;
using MelonLoader;
using MelonLoader.Preferences;

namespace BetterCasino.Config
{
    public static class Settings
    {
        private const int BetCeiling = 10_000_000;

        public static event Action<MelonPreferences_Entry>? Changed;

        public static MelonPreferences_Entry<bool> AlwaysOpen = null!;
        public static MelonPreferences_Entry<bool> Safehouse = null!;
        public static MelonPreferences_Entry<bool> SignChanger = null!;
        public static MelonPreferences_Entry<string> SignText = null!;
        public static MelonPreferences_Entry<bool> SlotMachines = null!;
        public static MelonPreferences_Entry<bool> TableGames = null!;

        public static MelonPreferences_Entry<bool> FreePlay = null!;
        public static MelonPreferences_Entry<int> SlotLowerLimit = null!;
        public static MelonPreferences_Entry<int> SlotUpperLimit = null!;

        public static MelonPreferences_Entry<int> WinChance = null!;
        public static MelonPreferences_Entry<bool> Penalties = null!;
        public static MelonPreferences_Entry<int> HoppingPenalty = null!;
        public static MelonPreferences_Entry<int> HoppingWindow = null!;
        public static MelonPreferences_Entry<int> StreakPenalty = null!;
        public static MelonPreferences_Entry<int> StreakThreshold = null!;

        public static MelonPreferences_Entry<int> BlackjackLowerLimit = null!;
        public static MelonPreferences_Entry<int> BlackjackUpperLimit = null!;
        public static MelonPreferences_Entry<int> RideTheBusLowerLimit = null!;
        public static MelonPreferences_Entry<int> RideTheBusUpperLimit = null!;

        public static MelonPreferences_Entry<int> RtbRedOrBlackChance = null!;
        public static MelonPreferences_Entry<int> RtbHigherOrLowerChance = null!;
        public static MelonPreferences_Entry<int> RtbInsideOrOutsideChance = null!;
        public static MelonPreferences_Entry<int> RtbSuitChance = null!;

        private static readonly List<(MelonPreferences_Entry Lower, MelonPreferences_Entry Upper)> Ranges = new();

        public static bool OddsOverridden => SlotMachines.Value && WinChance.Value >= 0;

        public static void Init()
        {
            var general = MelonPreferences.CreateCategory("BetterCasino", "Better Casino");
            AlwaysOpen = general.CreateEntry("AlwaysOpen", true, "Always Open",
                "Keeps the casino open around the clock instead of 4 PM to 5 AM.");
            Safehouse = general.CreateEntry("CasinoSafehouse", true, "Casino Safehouse",
                "Clears your wanted level while you are inside the casino.");
            SignChanger = general.CreateEntry("SignChangerEnabled", true, "Custom Sign",
                "Replaces the opening hours on the casino sign.");
            SignText = general.CreateEntry("SignText", "OPEN 24/7", "Sign Text",
                "Text shown on the casino sign when Custom Sign is on.");
            SlotMachines = general.CreateEntry("SlotMachineEnabled", true, "Slot Machine Tweaks",
                "Enables the custom bet ladder and odds for slot machines.");
            TableGames = general.CreateEntry("TableGamesEnabled", true, "Table Game Tweaks",
                "Enables custom bet limits for Blackjack and Ride the Bus, and the Ride the Bus odds.");

            var slots = MelonPreferences.CreateCategory("BetterCasino_Slots", "Slot Machines");
            FreePlay = slots.CreateEntry("FreePlay", false, "Free Play",
                "Adds a $0 bet for free practice spins. Free spins never pay out.");
            SlotLowerLimit = slots.CreateEntry("LowerLimit", 5, "Minimum Bet",
                "Smallest bet on the slot machine ladder.", validator: new ValueRange<int>(1, BetCeiling));
            SlotUpperLimit = slots.CreateEntry("UpperLimit", 100_000, "Maximum Bet",
                "Largest bet on the slot machine ladder.", validator: new ValueRange<int>(1, BetCeiling));

            var odds = MelonPreferences.CreateCategory("BetterCasino_SlotsOdds", "Slot Machine Odds");
            WinChance = odds.CreateEntry("ChancePercent", -1, "Win Chance (%)",
                "Percent chance that a spin pays out, 0 to 100. -1 keeps the game's own odds of about 31%.",
                validator: new ValueRange<int>(-1, 100));
            Penalties = odds.CreateEntry("PenaltiesEnabled", false, "Win Penalties",
                "Lowers the win chance for machine hopping and win streaks using the settings below.");
            HoppingPenalty = odds.CreateEntry("PenaltyHopPercent", 50, "Hopping Penalty (%)",
                "Win chance is cut by this percent right after switching machines.",
                validator: new ValueRange<int>(0, 100));
            HoppingWindow = odds.CreateEntry("PenaltyHopSeconds", 10, "Hopping Window (seconds)",
                "How long the hopping penalty lasts after switching machines.",
                validator: new ValueRange<int>(0, 600));
            StreakPenalty = odds.CreateEntry("PenaltyStreakPercent", 10, "Streak Penalty (%)",
                "Win chance drops by this percent for every win past the streak threshold.",
                validator: new ValueRange<int>(0, 100));
            StreakThreshold = odds.CreateEntry("PenaltyStreakThreshold", 3, "Streak Threshold",
                "Wins in a row allowed before the streak penalty starts.",
                validator: new ValueRange<int>(1, 100));

            var tables = MelonPreferences.CreateCategory("BetterCasino_Tables", "Table Games");
            BlackjackLowerLimit = tables.CreateEntry("BlackjackLowerLimit", 10, "Blackjack Minimum Bet",
                "Smallest bet allowed at the Blackjack table.", validator: new ValueRange<int>(1, BetCeiling));
            BlackjackUpperLimit = tables.CreateEntry("BlackjackUpperLimit", 5000, "Blackjack Maximum Bet",
                "Largest bet allowed at the Blackjack table.", validator: new ValueRange<int>(1, BetCeiling));
            RideTheBusLowerLimit = tables.CreateEntry("RideTheBusLowerLimit", 10, "Ride the Bus Minimum Bet",
                "Smallest bet allowed at the Ride the Bus table.", validator: new ValueRange<int>(1, BetCeiling));
            RideTheBusUpperLimit = tables.CreateEntry("RideTheBusUpperLimit", 5000, "Ride the Bus Maximum Bet",
                "Largest bet allowed at the Ride the Bus table.", validator: new ValueRange<int>(1, BetCeiling));

            var rtbOdds = MelonPreferences.CreateCategory("BetterCasino_RideTheBusOdds", "Ride the Bus Odds");
            RtbRedOrBlackChance = rtbOdds.CreateEntry("RedOrBlackChancePercent", -1, "Red or Black Chance (%)",
                "Percent chance that the Red or Black guess is correct, 0 to 100. -1 keeps the game's own odds.",
                validator: new ValueRange<int>(-1, 100));
            RtbHigherOrLowerChance = rtbOdds.CreateEntry("HigherOrLowerChancePercent", -1, "Higher or Lower Chance (%)",
                "Percent chance that the Higher or Lower guess is correct, 0 to 100. -1 keeps the game's own odds.",
                validator: new ValueRange<int>(-1, 100));
            RtbInsideOrOutsideChance = rtbOdds.CreateEntry("InsideOrOutsideChancePercent", -1, "Inside or Outside Chance (%)",
                "Percent chance that the Inside or Outside guess is correct, 0 to 100. -1 keeps the game's own odds.",
                validator: new ValueRange<int>(-1, 100));
            RtbSuitChance = rtbOdds.CreateEntry("SuitChancePercent", -1, "Suit Chance (%)",
                "Percent chance that the Suit guess is correct, 0 to 100. -1 keeps the game's own odds.",
                validator: new ValueRange<int>(-1, 100));

            Ranges.Add((SlotLowerLimit, SlotUpperLimit));
            Ranges.Add((BlackjackLowerLimit, BlackjackUpperLimit));
            Ranges.Add((RideTheBusLowerLimit, RideTheBusUpperLimit));

            MigrateLegacy(general);

            foreach (var range in Ranges)
                FixRange(range.Lower, range.Upper);

            foreach (var category in new[] { general, slots, odds, tables, rtbOdds })
                foreach (var entry in category.Entries)
                    entry.OnEntryValueChangedUntyped.Subscribe((_, _) => OnEntryChanged(entry));

            MelonPreferences.Save();
        }

        private static void OnEntryChanged(MelonPreferences_Entry entry)
        {
            foreach (var range in Ranges)
                if (entry == range.Lower || entry == range.Upper)
                    FixRange(range.Lower, range.Upper, entry == range.Upper);

            if (entry.Validator != null)
                ModManagerBridge.RequestUIRefresh();

            Changed?.Invoke(entry);
        }

        private static void FixRange(MelonPreferences_Entry lower, MelonPreferences_Entry upper, bool upperWins = false)
        {
            var min = (int)lower.BoxedValue;
            var max = (int)upper.BoxedValue;
            if (min <= max) return;

            if (upperWins) lower.BoxedValue = max;
            else upper.BoxedValue = min;
        }

        private static void MigrateLegacy(MelonPreferences_Category general)
        {
            var migrated = 0;

            bool Move<TOld, TNew>(string identifier, TOld legacyDefault, MelonPreferences_Entry<TNew> target, Func<TOld, TNew> convert)
            {
                var legacy = general.CreateEntry(identifier, legacyDefault, is_hidden: true, dont_save_default: true);
                if (EqualityComparer<TOld>.Default.Equals(legacy.Value, legacyDefault)) return false;

                target.Value = convert(legacy.Value);
                legacy.ResetToDefault();
                migrated++;
                return true;
            }

            static int Percent(float fraction) => (int)MathF.Round(fraction * 100f);
            static T Same<T>(T value) => value;

            Move("FreePlay", false, FreePlay, Same);
            Move("SlotMinBet", 10, SlotLowerLimit, Same);
            Move("SlotMaxBet", 5000, SlotUpperLimit, Same);
            if (Move("SlotWinRate", -1f, WinChance, Percent)) Penalties.Value = true;
            Move("MultiMachinePenalty", 0.5f, HoppingPenalty, Percent);
            Move("MultiMachineTimeWindow", 10f, HoppingWindow, seconds => (int)MathF.Round(seconds));
            Move("WinStreakPenalty", 0.9f, StreakPenalty, multiplier => Percent(1f - multiplier));
            Move("WinStreakThreshold", 3, StreakThreshold, Same);
            Move("BlackjackMinBet", 10, BlackjackLowerLimit, Same);
            Move("BlackjackMaxBet", 5000, BlackjackUpperLimit, Same);
            Move("RTBMinBet", 10, RideTheBusLowerLimit, Same);
            Move("RTBMaxBet", 5000, RideTheBusUpperLimit, Same);
            general.CreateEntry("SlotBetStep", 10, is_hidden: true, dont_save_default: true).ResetToDefault();

            if (migrated > 0)
                Core.Log.Msg($"Migrated {migrated} setting(s) from Better Casino 3.x.");
        }
    }
}
