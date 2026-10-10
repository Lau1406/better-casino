using BetterCasino.Casino;
using BetterCasino.Config;
using HarmonyLib;
using Il2CppScheduleOne.Map;

namespace BetterCasino.Features
{
    public static class AlwaysOpen
    {
        public static void Init()
        {
            Settings.Changed += entry =>
            {
                if (entry == Settings.AlwaysOpen) Refresh();
            };
        }

        private static void Refresh()
        {
            var zone = CasinoLocator.Zone;
            if (zone != null) zone.OnUncappedMinPass();
        }

        [HarmonyPatch(typeof(TimedAccessZone), nameof(TimedAccessZone.GetIsOpen))]
        private static class GetIsOpenPatch
        {
            private static bool Prefix(TimedAccessZone __instance, ref bool __result)
            {
                if (!Settings.AlwaysOpen.Value || !CasinoLocator.IsCasinoZone(__instance)) return true;

                __result = true;
                return false;
            }
        }
    }
}
