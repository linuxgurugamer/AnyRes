using System;
using System.Collections;
using UnityEngine;
using System.IO;

using System.Runtime.InteropServices;


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
                dirPath = KSPUtil.ApplicationRootPath.Replace("\\", "/") + "GameData/AnyRes/PluginData/";


                var files = AnyRes.UpdateFilesList(true);
                if (files == null)
                    Log.Error("files is null");
                if (files.Length == 1)
                {
                    LastSetRes = files[0].node;
                }
            }

            if (initial & LastSetRes != null)

            {
                AnyRes.SetScreenRes(LastSetRes, false);
            }
            LoadWinPos();
        }

        void LoadWinPos(string WinPosName = "WINPOS")
        {
            if (File.Exists(AnyRes.WinPosFileName(WinPosName)))
            {
                ConfigNode config = ConfigNode.Load(AnyRes.WinPosFileName(WinPosName));
                int Left;
                int.TryParse(config.GetValue("Left"), out Left);
                int Top;
                int.TryParse(config.GetValue("Top"), out Top);

                StartCoroutine(SetWindowPosition(Left, Top));
            }
        }


        //////////////////////////////////////////////////////////////////////
        ///

        // ******************************** user32.dll SetWindowPos ******************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="hWndInsertAfter"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="uFlags"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        // ******************************** user32.dll FindWindow, SetWindowPosition ******************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <param name="windowName"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string className, string windowName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public IEnumerator SetWindowPosition(int x, int y)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            SetWindowPos(FindWindow(null, Application.productName), 0, x, y, 0, 0, 5);
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