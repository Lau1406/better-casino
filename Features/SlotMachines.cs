using System;
using System.Collections.Generic;
using System.Linq;
using BetterCasino.Config;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppScheduleOne.Casino;
using Il2CppScheduleOne.Money;
using UnityEngine;

namespace BetterCasino.Features
{
    public static class SlotMachines
    {
        private static readonly int[] Steps = { 10, 25, 50 };

        private static int[]? _nativeBets;
        private static Il2CppStructArray<int>? _ladder;

        public static void Init()
        {
            Settings.Changed += entry =>
            {
                if (entry == Settings.SlotMachines || entry == Settings.FreePlay ||
                    entry == Settings.SlotLowerLimit || entry == Settings.SlotUpperLimit)
                    Rebuild();
            };
        }

        private static void Rebuild()
        {
            _ladder = null;
            if (_nativeBets == null) return;

            Install();
            foreach (var machine in UnityEngine.Object.FindObjectsOfType<SlotMachine>())
            {
                machine.currentBetIndex = Math.Clamp(machine.currentBetIndex, 0, SlotMachine.BetAmounts.Length - 1);

                if (Settings.SlotMachines.Value) UpdateLabel(machine);
                else if (machine.BetAmountLabel != null) machine.BetAmountLabel.text = MoneyManager.FormatAmount(machine.currentBetAmount);
            }
        }

        private static void Install()
        {
            if (_nativeBets == null)
            {
                var native = SlotMachine.BetAmounts;
                if (native == null || native.Length == 0) return;
                _nativeBets = native.ToArray();
            }

            SlotMachine.BetAmounts = Settings.SlotMachines.Value ? _ladder ??= BuildLadder() : _nativeBets;
        }

        private static Il2CppStructArray<int> BuildLadder()
        {
            var lower = Settings.SlotLowerLimit.Value;
            var upper = Settings.SlotUpperLimit.Value;
            var bets = new SortedSet<int> { lower, upper };

            foreach (var bet in _nativeBets!)
                if (bet >= lower && bet <= upper) bets.Add(bet);

            for (long magnitude = 1; magnitude <= upper; magnitude *= 10)
                foreach (var step in Steps)
                {
                    if (magnitude * step % 10 != 0) continue;

                    var bet = magnitude * step / 10;
                    if (bet >= lower && bet <= upper) bets.Add((int)bet);
                }

            if (Settings.FreePlay.Value) bets.Add(0);

            var ladder = new int[bets.Count];
            bets.CopyTo(ladder);
            Core.Log.Msg($"Slot bets: {FormatCompact(ladder[0])} to {FormatCompact(ladder[^1])} ({ladder.Length} steps).");
            return ladder;
        }

        private static void UpdateLabel(SlotMachine machine)
        {
            var label = machine.BetAmountLabel;
            if (label == null) return;

            if (!label.enableAutoSizing)
            {
                label.fontSizeMax = label.fontSize;
                label.fontSizeMin = label.fontSize * 0.5f;
                label.enableAutoSizing = true;
            }

            label.text = FormatCompact(machine.currentBetAmount);
        }

        public static string FormatCompact(int amount) => amount switch
        {
            <= 0 => "FREE",
            >= 1_000_000 => $"${amount / 1_000_000f:0.##}M",
            >= 1_000 => $"${amount / 1_000f:0.#}k",
            _ => $"${amount}"
        };

        [HarmonyPatch(typeof(SlotMachine), nameof(SlotMachine.Awake))]
        private static class AwakePatch
        {
            private static void Postfix() => Install();
        }

        [HarmonyPatch(typeof(SlotMachine), nameof(SlotMachine.RpcLogic___SetBetIndex_2681120339))]
        private static class SetBetIndexPatch
        {
            private static void Postfix(SlotMachine __instance)
            {
                if (Settings.SlotMachines.Value) UpdateLabel(__instance);
            }
        }
    }
}
