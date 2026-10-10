using System;
using BetterCasino.Config;
using HarmonyLib;
using Il2CppScheduleOne.Casino;
using Il2CppScheduleOne.Casino.UI;
using Il2CppScheduleOne.Money;

namespace BetterCasino.Features
{
    public static class TableGames
    {
        private static CasinoGameBetPanel? _panel;
        private static float _shownBet = -1f;
        private static string _shownText = string.Empty;

        public static void LateTick()
        {
            if (_panel == null) return;

            var game = _panel._gameController;
            var slider = _panel._betSlider;
            var label = _panel._betAmount;
            if (game == null || slider == null || label == null || !_panel._container.activeInHierarchy ||
                !TryGetLimits(game, out var lower, out var upper))
            {
                _panel = null;
                return;
            }

            var bet = Snap(slider.value, lower, upper);
            if (bet != slider.value) slider.SetValueWithoutNotify(bet);
            if (bet != game.LocalPlayerBet) game.SetLocalPlayerBet(bet);

            if (bet != _shownBet)
            {
                _shownBet = bet;
                _shownText = MoneyManager.FormatAmount(bet);
            }

            if (label.text != _shownText) label.text = _shownText;
        }

        private static bool TryGetLimits(CasinoGameController? game, out float lower, out float upper)
        {
            lower = upper = 0f;
            if (!Settings.TableGames.Value || game == null) return false;

            if (game.TryCast<BlackjackGameController>() != null)
            {
                lower = Settings.BlackjackLowerLimit.Value;
                upper = Settings.BlackjackUpperLimit.Value;
                return true;
            }

            if (game.TryCast<RTBGameController>() != null)
            {
                lower = Settings.RideTheBusLowerLimit.Value;
                upper = Settings.RideTheBusUpperLimit.Value;
                return true;
            }

            return false;
        }

        private static float Snap(float bet, float lower, float upper)
        {
            if (bet <= lower) return lower;
            if (bet >= upper) return upper;

            var step = Math.Max(1f, MathF.Pow(10f, MathF.Floor(MathF.Log10(bet)) - 1f));
            return Math.Clamp(MathF.Round(bet / step) * step, lower, upper);
        }

        [HarmonyPatch(typeof(CasinoGameBetPanel), nameof(CasinoGameBetPanel.Open))]
        private static class OpenPatch
        {
            private static void Postfix(CasinoGameBetPanel __instance, CasinoGameController game)
            {
                var slider = __instance._betSlider;
                if (slider == null || !TryGetLimits(game, out var lower, out var upper)) return;

                slider.minValue = lower;
                slider.maxValue = upper;
                slider.SetValueWithoutNotify(lower);
                game.SetLocalPlayerBet(lower);

                _panel = __instance;
                _shownBet = -1f;
            }
        }
    }
}
