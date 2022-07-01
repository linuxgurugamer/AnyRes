using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;  //Get Regex
using KSP.UI.Screens;

using System.Runtime.InteropServices;

using ToolbarControl_NS;
using ClickThroughFix;

namespace AnyRes
{


    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class AnyRes : MonoBehaviour
    {

        public static Rect anyresWinRect = new Rect(35, 99, 400, 275);
        public Rect deleteRect = new Rect((Screen.width - 200) / 2, (Screen.height - 100) / 2, 200, 100);

        public string nameString = "";
        public string xString = "1280";
        public string yString = "720";

        public int x = 1280;
        public int y = 720;

        const string LASTSETRES = "LastSetRes";

        public bool windowEnabled = false;
        public bool fullScreen = true;
        public bool reloadScene = false;

        ToolbarControl toolbarControl;

        string[] files;
        string file = "";
        string deleteFile = "";
        string deleteFileName = "";
        Vector2 scrollViewPos;
        bool deleteEnabled = false;
        bool confirmDeleteEnabled = false;


        void Start()
        {
#if false
            if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {

                anyresWinRect.x = Screen.width - 272;
                anyresWinRect.y = Screen.height - 231;

            }
            Debug.Log ("[AnyRes] Loaded, scene: " + HighLogic.LoadedScene);
#endif


            xString = GameSettings.SCREEN_RESOLUTION_WIDTH.ToString();
            yString = GameSettings.SCREEN_RESOLUTION_HEIGHT.ToString();
            fullScreen = GameSettings.FULLSCREEN;

            files = UpdateFilesList();

            Log.Info("SceneLoaded, scene: " + HighLogic.LoadedScene);
            toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(OnTrue, OnFalse,
                      ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW |
                      ApplicationLauncher.AppScenes.SPACECENTER |
                       ApplicationLauncher.AppScenes.SPH |
                      ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.VAB,
                      MODID,
                      "AnyResButton",
                      "AnyRes/textures/Toolbar_32",
                      "AnyRes/textures/Toolbar_24",
                      MODNAME);

        }

        internal const string MODID = "AnyRes_NS";
        internal const string MODNAME = "AnyRes";

        void OnTrue()
        {
            windowEnabled = true;
        }
        void OnFalse()
        {
            windowEnabled = false;
        }
        public void OnDisable()
        {
            OnDestroy();
        }
        public void OnDestroy()
        {

            if (toolbarControl != null)
            {
                toolbarControl.OnDestroy();
                Destroy(toolbarControl);
            }
        }


        void OnGUI()
        {
            if (windowEnabled)
            {
                if (toolbarControl != null)
                {
                    if (HighLogic.CurrentGame.Parameters.CustomParams<AR>().useKSPSkin)
                        GUI.skin = HighLogic.Skin;
                }

                if (anyresWinRect.x + anyresWinRect.width > Screen.width)
                    anyresWinRect.x = Screen.width - anyresWinRect.width;
                if (anyresWinRect.y + anyresWinRect.height > Screen.height)
                    anyresWinRect.y = Screen.height - anyresWinRect.height;


                anyresWinRect.x = Math.Max(anyresWinRect.x, 0);
                anyresWinRect.y = Math.Max(anyresWinRect.y, 0);

                anyresWinRect = ClickThruBlocker.GUIWindow(09271, anyresWinRect, GUIActive, "AnyRes");

            }
            if (confirmDeleteEnabled)
                deleteRect = ClickThruBlocker.GUIWindow(09276, deleteRect, ConfirmDelete, "Confirm");
        }

        void GUIActive(int windowID)
        {
            GUILayout.BeginHorizontal();

            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Name: ");
                    nameString = GUILayout.TextField(nameString);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Width: ");
                    xString = GUILayout.TextField(xString);
                    xString = Regex.Replace(xString, @"[^0-9]", "");
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Height: ");
                    yString = GUILayout.TextField(yString);
                    yString = Regex.Replace(yString, @"[^0-9]", "");
                }
                fullScreen = GUILayout.Toggle(fullScreen, "Fullscreen");
                //			reloadScene = GUILayout.Toggle (reloadScene, "Reload scene");
                if (GUILayout.Button("Set Screen Resolution"))
                {

                    if (xString != null && yString != null)
                    {

                        x = Convert.ToInt32(xString);
                        y = Convert.ToInt32(yString);

                        if (x > 0 && y > 0)
                        {

                            GameSettings.SCREEN_RESOLUTION_HEIGHT = y;
                            GameSettings.SCREEN_RESOLUTION_WIDTH = x;
                            GameSettings.FULLSCREEN = fullScreen;
                            GameSettings.SaveSettings();
                            Screen.SetResolution(x, y, fullScreen);

                            SaveDataConfig(x, y, fullScreen);


                            Debug.Log("[AnyRes] Set screen resolution");
                        }
                        else
                        {

                            ScreenMessages.PostScreenMessage("One or both of your values is too small.  Please enter a valid value.", 1, ScreenMessageStyle.UPPER_CENTER);
                        }
                    }
                    else
                    {
                        ScreenMessages.PostScreenMessage("The values you have set are invalid.  Please set a valid value.", 1, ScreenMessageStyle.UPPER_CENTER);
                    }

                }
                if (nameString == "")
                    GUI.enabled = false;
                if (GUILayout.Button("Save"))
                {
                    var newName = nameString;
                    var newX = xString;
                    var newY = yString;
                    var newFullscreen = fullScreen;

                    SaveConfig(newName, newX, newY, newFullscreen);
                    ScreenMessages.PostScreenMessage("Preset saved.  You can change the preset later by using the same name in this editor.", 5, ScreenMessageStyle.UPPER_CENTER);
                    files = UpdateFilesList();

                }


                if (files.Length == 0)
                    GUI.enabled = false;
                else
                    GUI.enabled = true;
                if (deleteEnabled)
                {
                    if (GUILayout.Button("Disable Delete"))
                    {
                        deleteEnabled = false;
                    }

                }
                else
                {
                    if (GUILayout.Button("Enable Delete"))
                    {
                        deleteEnabled = true;
                    }
                }

                if (HighLogic.CurrentGame.Parameters.CustomParams<AR>().saveWinPos)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Save Win Pos"))
                        {
                            SaveWinPos();
                        }

                        if (GUILayout.Button("Clear Win Pos"))
                        {
                            SaveWinPos(clear: true);
                        }
                    }
                }


                if (GUILayout.Button("Close"))
                {
                    toolbarControl.SetFalse(true);
                }
            }

            using (new GUILayout.VerticalScope())
            {
                scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);
                for (int i = files.Length - 1; i >= 0; --i)
                {

                    file = files[i];

                    ConfigNode config = ConfigNode.Load(file);
                    if (deleteEnabled)
                    {
                        if (GUILayout.Button("Delete " + config.GetValue("name")))
                        {
                            confirmDeleteEnabled = true;
                            deleteFile = file;
                            deleteFileName = config.GetValue("name");
                        }

                    }
                    else
                    {
                        if (GUILayout.Button(config.GetValue("name")))
                        {
                            SetScreenRes(config);
                            GameSettings.SaveSettings();

                            Debug.Log("[AnyRes] Set screen resolution from preset");
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();

            if (GUI.Button(new Rect(anyresWinRect.width - 18, 3f, 15f, 15f), new GUIContent("X")))
            {
                toolbarControl.SetFalse(true);
            }

            GUI.DragWindow();
        }

        public static void SetScreenRes(ConfigNode config, bool saveConfig = true)
        {
            int xVal;
            int.TryParse(config.GetValue("x"), out xVal);
            int yVal;
            int.TryParse(config.GetValue("y"), out yVal);
            bool fullscreen;
            bool.TryParse(config.GetValue("fullscreen"), out fullscreen);
            GameSettings.SCREEN_RESOLUTION_HEIGHT = yVal;
            GameSettings.SCREEN_RESOLUTION_WIDTH = xVal;
            GameSettings.FULLSCREEN = fullscreen;
            Screen.SetResolution(xVal, yVal, fullscreen);
            if (saveConfig)
            {
                SaveDataConfig(xVal, yVal, fullscreen);
#if false
                SaveConfig(LASTSETRES, xVal.ToString(), yVal.ToString(), fullscreen);
                var files = UpdateFilesList(true);
                if (files.Length == 1)
                {
                    SetInitialRes.LastSetRes = ConfigNode.Load(files[0]);
                }
#endif
            }
        }

        static void SaveDataConfig(int xVal, int yVal, bool fullscreen)
        {
            SaveConfig(LASTSETRES, xVal.ToString(), yVal.ToString(), fullscreen);
            var files = UpdateFilesList(true);
            if (files.Length == 1)
            {
                SetInitialRes.LastSetRes = ConfigNode.Load(files[0]);
            }

        }

        static void SaveConfig(string newName, string newX, string newY, bool newFullscreen)
        {
            ConfigNode config = new ConfigNode(newName);
            config.AddValue("name", newName);
            config.AddValue("x", newX);
            config.AddValue("y", newY);
            config.AddValue("fullscreen", newFullscreen.ToString());
            config.Save(KSPUtil.ApplicationRootPath.Replace("\\", "/") + "GameData/AnyRes/PluginData/" + newName + ".cfg");
        }

        internal static string WinPosFileName(string WinPosName)
        {
            return KSPUtil.ApplicationRootPath.Replace("\\", "/") + "GameData/AnyRes/PluginData/" + WinPosName + ".cfg";

        }

        void SaveWinPos(string WinPosName = "WINPOS", bool clear = false)
        {
            if (!clear)
            {
                IntPtr hWnd = FindWindow(null, Application.productName);

                GetWindowRect(hWnd, ref rect);

                ConfigNode config = new ConfigNode(WinPosName);
                config.AddValue("name", "WINPOS");
                config.AddValue("left", rect.Left);
                config.AddValue("top", rect.Top);
                config.Save(WinPosFileName(WinPosName));
            }
            else
                if (File.Exists(WinPosFileName(WinPosName)))
                File.Delete(WinPosFileName(WinPosName));
        }


        void ConfirmDelete(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Confirm delete of " + deleteFileName);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
            {
                deleteEnabled = false;
                confirmDeleteEnabled = false;
            }
            if (GUILayout.Button("Yes"))
            {
                //deleteEnabled = false;
                confirmDeleteEnabled = false;
                System.IO.File.Delete(deleteFile);
                files = UpdateFilesList();
            }
            GUILayout.EndHorizontal();
        }

        internal static string[] UpdateFilesList(bool LastRes = false)
        {
            var files = Directory.GetFiles(SetInitialRes.dirPath, "*.cfg");
            List<string> flist = new List<string>();
            foreach (var f in files)
            {
                Log.Info("file: " + f);
                if (LastRes)
                {
                    if (f == (SetInitialRes.dirPath + LASTSETRES + ".cfg"))
                        flist.Add(f);
                }
                else
                {
                    if (f != (SetInitialRes.dirPath + LASTSETRES + ".cfg"))
                        flist.Add(f);
                }
            }
            return flist.ToArray();
        }

        //////////////////////////////////////////////////////////////////////
        ///

        // ******************************** user32.dll FindWindow, SetWindowPosition ******************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <param name="windowName"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string className, string windowName);

        // ******************************** user32.dll GetWindowRect ******************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpRect"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        static RECT rect = new RECT();
    }
}

