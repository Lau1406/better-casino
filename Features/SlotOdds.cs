using System;
using System.Collections.Generic;
using BetterCasino.Config;
using HarmonyLib;
using Il2CppFishNet.Connection;
using Il2CppScheduleOne.Casino;
using UnityEngine;
using Random = System.Random;
using Symbol = Il2CppScheduleOne.Casino.SlotMachine.ESymbol;

namespace BetterCasino.Features
{
    public static class SlotOdds
    {
        private const int FruitCount = 4;

        private static readonly (SlotMachine.EOutcome Outcome, int Weight)[] WinTiers =
        {
            (SlotMachine.EOutcome.Jackpot, 1),
            (SlotMachine.EOutcome.BigWin, 1),
            (SlotMachine.EOutcome.SmallWin, FruitCount),
            (SlotMachine.EOutcome.MiniWin, 60)
        };

        private static readonly Random Rng = new();
        private static readonly Queue<Symbol> Planned = new();

        private static int _lastMachine;
        private static float _switchedAt = float.NegativeInfinity;
        private static int _streak;

        private static float EffectiveChance()
        {
            var chance = Settings.WinChance.Value / 100f;
            if (!Settings.Penalties.Value) return chance;

            if (Time.time - _switchedAt < Settings.HoppingWindow.Value)
                chance *= 1f - Settings.HoppingPenalty.Value / 100f;

            var excess = _streak - Settings.StreakThreshold.Value;
            if (excess >= 0)
                chance *= MathF.Pow(1f - Settings.StreakPenalty.Value / 100f, excess + 1);

            return Math.Clamp(chance, 0f, 1f);
        }

        private static void Plan(SlotMachine machine)
        {
            var id = machine.GetInstanceID();
            if (id != _lastMachine)
            {
                if (_lastMachine != 0) _switchedAt = Time.time;
                _lastMachine = id;
            }

            var reels = machine.Reels?.Length ?? 3;
            var win = Rng.NextDouble() < EffectiveChance();
            _streak = win ? _streak + 1 : 0;

            foreach (var symbol in win ? Winning(PickTier(), reels) : Losing(reels))
                Planned.Enqueue(symbol);
        }

        private static SlotMachine.EOutcome PickTier()
        {
            var total = 0;
            foreach (var tier in WinTiers) total += tier.Weight;

            var roll = Rng.Next(total);
            foreach (var (outcome, weight) in WinTiers)
            {
                if (roll < weight) return outcome;
                roll -= weight;
            }

            return SlotMachine.EOutcome.MiniWin;
        }

        private static Symbol[] Winning(SlotMachine.EOutcome tier, int reels)
        {
            var symbols = new Symbol[reels];
            switch (tier)
            {
                case SlotMachine.EOutcome.Jackpot:
                    Array.Fill(symbols, Symbol.Seven);
                    break;
                case SlotMachine.EOutcome.BigWin:
                    Array.Fill(symbols, Symbol.Bell);
                    break;
                case SlotMachine.EOutcome.SmallWin:
                    Array.Fill(symbols, (Symbol)Rng.Next(FruitCount));
                    break;
                default:
                    do
                    {
                        for (var i = 0; i < reels; i++) symbols[i] = (Symbol)Rng.Next(FruitCount);
                    } while (reels > 1 && IsUniform(symbols));
                    break;
            }

            return symbols;
        }

        private static Symbol[] Losing(int reels)
        {
            var symbols = new Symbol[reels];
            do
            {
                for (var i = 0; i < reels; i++) symbols[i] = (Symbol)Rng.Next(6);
            } while (reels > 1 && (IsUniform(symbols) || AllFruit(symbols)));

            return symbols;
        }

        private static bool IsUniform(Symbol[] symbols) => Array.TrueForAll(symbols, s => s == symbols[0]);

        private static bool AllFruit(Symbol[] symbols) => Array.TrueForAll(symbols, s => (int)s < FruitCount);

        [HarmonyPatch(typeof(SlotMachine), nameof(SlotMachine.RpcLogic___SendStartSpin_2681120339))]
        private static class SendStartSpinPatch
        {
            private static void Prefix(SlotMachine __instance, NetworkConnection spinner)
            {
                Planned.Clear();
                if (!Settings.OddsOverridden || __instance.IsSpinning) return;
                if (spinner == null || !spinner.IsLocalClient) return;

                Plan(__instance);
            }

            private static void Postfix() => Planned.Clear();
        }

        [HarmonyPatch(typeof(SlotMachine), nameof(SlotMachine.GetRandomSymbol))]
        private static class GetRandomSymbolPatch
        {
            private static bool Prefix(ref Symbol __result)
            {
                if (Planned.Count == 0) return true;

                __result = Planned.Dequeue();
                return false;
            }
        }
    }
}
