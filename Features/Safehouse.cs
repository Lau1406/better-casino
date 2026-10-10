using BetterCasino.Casino;
using BetterCasino.Config;
using Il2CppScheduleOne.PlayerScripts;
using UnityEngine;

namespace BetterCasino.Features
{
    public static class Safehouse
    {
        private const float CheckInterval = 0.5f;

        private static float _nextCheck;

        public static void Tick()
        {
            if (!Settings.Safehouse.Value || Time.unscaledTime < _nextCheck) return;
            _nextCheck = Time.unscaledTime + CheckInterval;

            var player = Player.Local;
            if (player == null || !CasinoLocator.Resolve()) return;

            var crime = player.CrimeData;
            if (crime == null) return;

            var level = crime.CurrentPursuitLevel;
            if (level == PlayerCrimeData.EPursuitLevel.None) return;
            if (!CasinoLocator.Area.Contains(player.transform.position)) return;

            crime.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.None);
            crime.ClearCrimes();
            crime.BodySearchPending = false;
            if (crime.onPursuitEscapedSound != null) crime.onPursuitEscapedSound.Play();

            Core.Log.Msg($"Safehouse cleared wanted level ({level}).");
        }
    }
}
