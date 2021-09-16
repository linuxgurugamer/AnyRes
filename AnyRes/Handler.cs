using UnityEngine;

namespace AnyRes
{

    public class SetInitialRes : MonoBehaviour
    {
        internal static string dirPath;
        static bool initialResSet = false;
        internal static ConfigNode LastSetRes = null;
        internal void DoStart(bool initial)
        {
            if (!initialResSet)
            {
                initialResSet = initial;
                Log.Info("SetInitialRes");
                dirPath = KSPUtil.ApplicationRootPath.Replace("\\", "/") + "GameData/AnyRes/PluginData/";


                var files = AnyRes.UpdateFilesList(true);
                if (files.Length == 1)
                {
                    LastSetRes = ConfigNode.Load(files[0]);
                }
            }
            if (LastSetRes != null)
                AnyRes.SetScreenRes(LastSetRes, false);


        }
    }

    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class SetInitialResInstant : SetInitialRes
    {
        void Start()
        {
            DoStart(true);
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class SetInitialResMainMenu : SetInitialRes
    {
        void Start()
        {
            DoStart(false);
        }
    }


    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class AllGameScenesResMainMenu : SetInitialRes
    {
        void Start()
        {
            DoStart(false);
        }

        void OnApplicationFocus(bool hasFocus)
        {
            DoStart(false);
        }
    }
}