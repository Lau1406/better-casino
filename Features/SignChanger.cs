using System.Collections;
using System.Collections.Generic;
using BetterCasino.Config;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;

namespace BetterCasino.Features
{
    public static class SignChanger
    {
        private const string HoursText = "4PM-5AM";
        private const int ScanAttempts = 12;
        private const float ScanInterval = 5f;

        private static readonly List<(TMP_Text Label, string Original)> Signs = new();
        private static object? _scan;

        public static void Init()
        {
            Settings.Changed += entry =>
            {
                if (entry == Settings.SignChanger || entry == Settings.SignText) Apply();
            };
        }

        public static void OnSceneLoaded(bool gameplay)
        {
            Signs.Clear();
            if (_scan != null) MelonCoroutines.Stop(_scan);
            _scan = gameplay ? MelonCoroutines.Start(Scan()) : null;
        }

        private static IEnumerator Scan()
        {
            for (var attempt = 0; attempt < ScanAttempts && Signs.Count == 0; attempt++)
            {
                yield return new WaitForSeconds(ScanInterval);

                foreach (var label in Object.FindObjectsOfType<TMP_Text>())
                    if (IsHoursSign(label.text)) Signs.Add((label, label.text));
            }

            _scan = null;
            if (Signs.Count == 0) yield break;

            Core.Log.Msg($"Found {Signs.Count} casino sign(s).");
            Apply();
        }

        private static bool IsHoursSign(string? text) =>
            text != null && text.Replace(" ", "").Trim().Equals(HoursText, System.StringComparison.OrdinalIgnoreCase);

        private static void Apply()
        {
            var custom = Settings.SignChanger.Value ? Settings.SignText.Value : null;

            foreach (var (label, original) in Signs)
                if (label != null) label.text = custom ?? original;
        }
    }
}
