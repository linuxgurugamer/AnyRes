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
using static Targeting;

namespace AnyRes
{


    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class AnyRes : MonoBehaviour
    {

        public static Rect anyresWinRect = new Rect(35, 99, 400, 320);
        public Rect deleteRect = new Rect((Screen.width - 200) / 2, (Screen.height - 100) / 2, 200, 120);

        public string nameString = "";
        public string xString = "1280";
        public string yString = "720";
        public string sString = "1.0";

        public int x = 1280;
        public int y = 720;
        public float s = 1.0f;

        const string LASTSETRES = "LastSetRes";

        public bool windowEnabled = false;
        public bool fullScreen = true;
        public static double highestUIscale = 1 + .0f;

        ToolbarControl toolbarControl;

        internal class ResConfig
        {
            internal string file;
            internal ConfigNode node = new ConfigNode();
        }
        //static string[] files;

        internal static ResConfig[] resConfigs;

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

            resConfigs = UpdateFilesList();

            Log.Info("SceneLoaded, scene: " + HighLogic.LoadedScene);
            Debug.Log("[AnyRes] OnStart() Scene Loaded: " + HighLogic.LoadedScene);
            SetScreenRes(ConfigNode.Load(SetInitialRes.dirPath + HighLogic.LoadedScene + ".cfg"), true);

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

            xString = GameSettings.SCREEN_RESOLUTION_WIDTH.ToString();
            yString = GameSettings.SCREEN_RESOLUTION_HEIGHT.ToString();
            sString = GameSettings.UI_SCALE.ToString();
            fullScreen = GameSettings.FULLSCREEN;

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
                // Fixes issue with changing UI Scaling while loading existing scene
                // The problem is with scaling UP while a scene is loaded or loading
                // OnDestroy, set UI Scaling to highest anticipated level so the next scene will be the same or lower scale
                Debug.Log("[AnyRes] OnDestroy - Set UI Scale to highest: " + highestUIscale);
                GameSettings.UI_SCALE = (float)highestUIscale;
                GameSettings.SaveSettings();
                GameSettings.ApplySettings();

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
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("UI_Scale: ");
                    sString = GUILayout.TextField(sString);
                    sString = Regex.Replace(sString, @"[^0-9\.]", "");
                }
                fullScreen = GUILayout.Toggle(fullScreen, "Fullscreen");

                if (GUILayout.Button("Set Resolution/Scale"))
                {

                    if (xString != null && yString != null && sString != null)
                    {

                        x = Convert.ToInt32(xString);
                        y = Convert.ToInt32(yString);
                        s = Convert.ToSingle(sString);

                        if (x > 0 && y > 0 && s > 0)
                        {

                            GameSettings.SCREEN_RESOLUTION_HEIGHT = y;
                            GameSettings.SCREEN_RESOLUTION_WIDTH = x;
                            GameSettings.UI_SCALE = s;
                            GameSettings.FULLSCREEN = fullScreen;
                            GameSettings.SaveSettings();
                            GameSettings.ApplySettings();
                            Log.Info("GUIActive.SetResolution, x: " + x + ", y: " + y + ", s: " + s + ", fullScreen: " + fullScreen);
                            Screen.SetResolution(x, y, fullScreen);

                            SaveDataConfig(x, y, s, fullScreen);

                            Debug.Log("[AnyRes] Set screen resolution");

                            xString = GameSettings.SCREEN_RESOLUTION_WIDTH.ToString();
                            yString = GameSettings.SCREEN_RESOLUTION_HEIGHT.ToString();
                            sString = GameSettings.UI_SCALE.ToString();
                            fullScreen = GameSettings.FULLSCREEN;
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
                    var newS = sString;
                    var newFullscreen = fullScreen;

                    SaveConfig(newName, newX, newY, newS, newFullscreen);
                    ScreenMessages.PostScreenMessage("Preset saved.  You can change the preset later by using the same name in this editor.", 5, ScreenMessageStyle.UPPER_CENTER);
                    resConfigs = UpdateFilesList();

                }


                if (resConfigs.Length == 0)
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
                for (int i = resConfigs.Length - 1; i >= 0; --i)
                {
                    if (deleteEnabled)
                    {
                        if (GUILayout.Button("Delete " + resConfigs[i].node.GetValue("name")))
                        {
                            confirmDeleteEnabled = true;
                            deleteFile = resConfigs[i].file;
                            deleteFileName = resConfigs[i].node.GetValue("name");
                        }

                    }
                    else
                    {
                        if (GUILayout.Button(resConfigs[i].node.GetValue("name")))
                        {
                            SetScreenRes(resConfigs[i].node);
                            SetInitialRes.LastSetRes = resConfigs[i].node;
                            GameSettings.SaveSettings();

                            Debug.Log("[AnyRes] Set screen resolution from preset");

                            nameString = GUILayout.TextField(resConfigs[i].node.GetValue("name"));
                            xString = GameSettings.SCREEN_RESOLUTION_WIDTH.ToString();
                            yString = GameSettings.SCREEN_RESOLUTION_HEIGHT.ToString();
                            sString = GameSettings.UI_SCALE.ToString();
                            fullScreen = GameSettings.FULLSCREEN;
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
            float sVal;
            float.TryParse(config.GetValue("scale"), out sVal);
            bool fullscreen;
            bool.TryParse(config.GetValue("fullscreen"), out fullscreen);
            GameSettings.SCREEN_RESOLUTION_HEIGHT = yVal;
            GameSettings.SCREEN_RESOLUTION_WIDTH = xVal;
            GameSettings.UI_SCALE = sVal;
            GameSettings.FULLSCREEN = fullscreen;
            Log.Info("SetScreenRes.SetResolution, xVal: " + xVal + ", yVal: " + yVal + ", fullscreen: " + fullscreen);
            Screen.SetResolution(xVal, yVal, fullscreen);
            GameSettings.SaveSettings();
            GameSettings.ApplySettings();

            if (saveConfig)
            {
                SaveDataConfig(xVal, yVal, sVal, fullscreen);
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

        static void SaveDataConfig(int xVal, int yVal, float sVal, bool fullscreen)
        {
            SaveConfig(LASTSETRES, xVal.ToString(), yVal.ToString(), sVal.ToString(), fullscreen);
            var files = UpdateFilesList(true);
            if (files.Length == 1)
            {
                SetInitialRes.LastSetRes = resConfigs[0].node;

            }

        }

        static void SaveConfig(string newName, string newX, string newY, string newS, bool newFullscreen)
        {
            ConfigNode config = new ConfigNode(newName);
            config.AddValue("name", newName);
            config.AddValue("x", newX);
            config.AddValue("y", newY);
            config.AddValue("scale", newS);
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
                resConfigs = UpdateFilesList();
            }
            GUILayout.EndHorizontal();
        }

        internal static ResConfig[] UpdateFilesList(bool LastRes = false)
        {
            var files = Directory.GetFiles(SetInitialRes.dirPath, "*.cfg");
            List<ResConfig> flist = new List<ResConfig>();
            

            foreach (var f in files)
            {
                ResConfig cfg = new ResConfig();

                cfg.file = f;
                cfg.node = ConfigNode.Load(f);

                if (LastRes)
                {
                    if (f == (SetInitialRes.dirPath + LASTSETRES + ".cfg"))
                        flist.Add(cfg);
                }
                else
                {
                    if (f != (SetInitialRes.dirPath + LASTSETRES + ".cfg"))
                        flist.Add(cfg);
                }
                // Determine the highest UI scaling in the Preset files so that upscaling can be done OnDestroy and only downscaling on loading new scenes
                float sVal;
                float.TryParse(ConfigNode.Load(f).GetValue("scale"), out sVal);
                if (sVal > highestUIscale) { highestUIscale = sVal; }
                // Debug.Log("[AnyRes] File added: " + f + " with scale " + sVal + " - Highest UI Scale currently: " + highestUIscale); // Debug
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

