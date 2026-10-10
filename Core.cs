using BetterCasino.Casino;
using BetterCasino.Config;
using BetterCasino.Features;
using MelonLoader;

namespace BetterCasino
{
    public class Core : MelonMod
    {
        private const string GameplayScene = "Main";

        public static MelonLogger.Instance Log { get; private set; } = null!;

        public override void OnInitializeMelon()
        {
            Log = LoggerInstance;

            Settings.Init();
            AlwaysOpen.Init();
            SignChanger.Init();
            SlotMachines.Init();

            Log.Msg($"Better Casino v{Info.Version} loaded.");
        }

        public override void OnLateInitializeMelon()
        {
            Log.Msg(ModManagerBridge.Available
                ? "Mod Manager detected, settings can be changed in-game."
                : "Settings can be changed in UserData/MelonPreferences.cfg.");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            CasinoLocator.Reset();
            SignChanger.OnSceneLoaded(sceneName == GameplayScene);
        }

        public override void OnUpdate() => Safehouse.Tick();

        public override void OnLateUpdate() => TableGames.LateTick();
    }
}
