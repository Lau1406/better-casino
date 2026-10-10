using Il2CppScheduleOne.Casino;
using Il2CppScheduleOne.Map;
using UnityEngine;

namespace BetterCasino.Casino
{
    public static class CasinoLocator
    {
        private const float RetryInterval = 5f;
        private const float MaxZoneDistance = 40f;
        private static readonly Vector3 AreaPadding = new(16f, 8f, 16f);

        private static float _nextAttempt;
        private static TimedAccessZone? _zone;

        public static bool Found { get; private set; }
        public static Bounds Area { get; private set; }

        public static TimedAccessZone? Zone
        {
            get
            {
                Resolve();
                return _zone;
            }
        }

        public static void Reset()
        {
            Found = false;
            _zone = null;
            _nextAttempt = 0f;
        }

        public static bool IsCasinoZone(TimedAccessZone zone)
        {
            Resolve();
            return _zone != null && _zone.Pointer == zone.Pointer;
        }

        public static bool Resolve()
        {
            if (Found) return true;
            if (Time.unscaledTime < _nextAttempt) return false;
            _nextAttempt = Time.unscaledTime + RetryInterval;

            if (!TryGetArea(out var area)) return false;

            Area = area;
            _zone = FindZone(area.center);
            Found = true;
            Core.Log.Msg(_zone != null
                ? $"Casino located at {area.center}, access zone '{_zone.name}'."
                : $"Casino located at {area.center}, no access zone found nearby.");
            return true;
        }

        private static bool TryGetArea(out Bounds area)
        {
            area = default;
            var any = false;

            foreach (var machine in Object.FindObjectsOfType<SlotMachine>())
                Encapsulate(ref area, ref any, machine.transform.position);
            foreach (var table in Object.FindObjectsOfType<CasinoGameController>())
                Encapsulate(ref area, ref any, table.transform.position);

            if (!any) return false;

            area.Expand(AreaPadding);
            return true;
        }

        private static void Encapsulate(ref Bounds area, ref bool any, Vector3 point)
        {
            if (any)
            {
                area.Encapsulate(point);
                return;
            }

            area = new Bounds(point, Vector3.zero);
            any = true;
        }

        private static TimedAccessZone? FindZone(Vector3 center)
        {
            TimedAccessZone? best = null;
            var bestDistance = MaxZoneDistance * MaxZoneDistance;

            foreach (var zone in Object.FindObjectsOfType<TimedAccessZone>())
            {
                if (zone.TryCast<DarkMarketAccessZone>() != null) continue;

                var distance = (ZonePosition(zone) - center).sqrMagnitude;
                if (distance >= bestDistance) continue;

                best = zone;
                bestDistance = distance;
            }

            return best;
        }

        private static Vector3 ZonePosition(AccessZone zone)
        {
            var doors = zone.Doors;
            return doors != null && doors.Length > 0 && doors[0] != null
                ? doors[0].transform.position
                : zone.transform.position;
        }
    }
}
