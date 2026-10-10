using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MelonLoader;

namespace BetterCasino.Config
{
    public static class ModManagerBridge
    {
        private static bool? _available;

        public static bool Available =>
            _available ??= MelonBase.RegisteredMelons.Any(m => m.Info.Name == "Mod Manager & Phone App");

        public static void RequestUIRefresh()
        {
            if (!Available) return;

            try
            {
                Refresh();
            }
            catch (Exception ex)
            {
                _available = false;
                Core.Log.Warning($"Mod Manager refresh failed: {ex.Message}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Refresh() => ModManagerPhoneApp.ModSettingsEvents.RequestUIRefresh();
    }
}
