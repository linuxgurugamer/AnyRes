﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;  //Get Regex
using KSP.UI.Screens;

using System.Runtime.InteropServices;

using ToolbarControl_NS;
using ClickThroughFix;
using KSP.UI;

namespace AnyRes
{


    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class AnyRes : MonoBehaviour
    {

        public static Rect anyresWinRect = new Rect(35, 99, 480, 360);
        public Rect deleteRect = new Rect((Screen.width - 200) / 2, (Screen.height - 100) / 2, 200, 150);

        public string nameString = "";
        public string xString = "1280";
        public string yString = "720";

        public int x = 1280;
        public int y = 720;

        const string LASTSETRES = "LastSetRes";

        public bool windowEnabled = false;
        public bool fullScreen = true;
        public static double highestUIscale = 1 + .0f;

        ToolbarControl toolbarControl;

        static float scale;
        static float appScale;

        static void SetResolution(int width, int height, float uiScale, float app_Scale, bool fullscreen)
        {
            GameSettings.SCREEN_RESOLUTION_HEIGHT = height;
            GameSettings.SCREEN_RESOLUTION_WIDTH = width;
            GameSettings.UI_SCALE = scale;
            GameSettings.UI_SCALE_APPS = app_Scale;
            GameSettings.FULLSCREEN = fullscreen;


            Screen.SetResolution(width, height, fullscreen);
            UIMasterController.Instance.SetScale(uiScale);
            UIMasterController.Instance.SetAppScale(app_Scale * uiScale);
            scale = uiScale;
            appScale = app_Scale;
        }


        internal class ResConfig
        {
            internal string file;
            internal ConfigNode node = new ConfigNode();
        }
        //static string[] files;

        internal static ResConfig[] resConfigs;

        //string file = "";
        string deleteFile = "";
        string deleteFileName = "";
        Vector2 scrollViewPos;
        bool deleteEnabled = false;
        bool confirmDeleteEnabled = false;


        void Start()
        {
#if DEBUG
            Log.Info("SceneLoaded, scene: " + HighLogic.LoadedScene);
#endif
            // if scene config file does not exist, create it based on current values
            string sceneConfigFile = SetInitialRes.dirPath + HighLogic.LoadedScene + ".cfg";
            if (!File.Exists(sceneConfigFile))
            {
#if DEBUG
                Log.Info("[AnyRes] Creating " + HighLogic.LoadedScene + " config file");
#endif
                xString = GameSettings.SCREEN_RESOLUTION_WIDTH.ToString();
                yString = GameSettings.SCREEN_RESOLUTION_HEIGHT.ToString();
                //sString = GameSettings.UI_SCALE.ToString();
                scale = GameSettings.UI_SCALE;
                appScale = GameSettings.UI_SCALE_APPS;
                fullScreen = GameSettings.FULLSCREEN;
                SaveConfig(HighLogic.LoadedScene + "", xString, yString, scale.ToString(), appScale.ToString(), fullScreen);
            }
#if DEBUG
            Log.Info("[AnyRes] Setting SCENE config for " + HighLogic.LoadedScene);
#endif
            SetScreenRes(ConfigNode.Load(sceneConfigFile), true);

            resConfigs = UpdateFilesList();

#if DEBUG
            Debug.Log("[AnyRes] Creating Toolbar for scene " + HighLogic.LoadedScene);
#endif
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

            nameString = HighLogic.LoadedScene + "";
            xString = GameSettings.SCREEN_RESOLUTION_WIDTH.ToString();
            yString = GameSettings.SCREEN_RESOLUTION_HEIGHT.ToString();
            scale = GameSettings.UI_SCALE;
            appScale = GameSettings.UI_SCALE_APPS;
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
#if DEBUG
                Debug.Log("[AnyRes] OnDestroy - Set UI Scale to highest: " + highestUIscale);
#endif
                GameSettings.UI_SCALE = (float)Math.Max(highestUIscale, 1.0);
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
            if (Screen.width != GameSettings.SCREEN_RESOLUTION_WIDTH || Screen.height != GameSettings.SCREEN_RESOLUTION_HEIGHT)
            {
                SetResolution(GameSettings.SCREEN_RESOLUTION_WIDTH, GameSettings.SCREEN_RESOLUTION_HEIGHT,
                   GameSettings.UI_SCALE, GameSettings.UI_SCALE_APPS, GameSettings.FULLSCREEN);
            }

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
                if (HighLogic.CurrentGame.Parameters.CustomParams<AR>().useUIScale)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("UI Scale: ");
                        scale = GUILayout.HorizontalSlider(scale, .8f, 2.0f, GUILayout.MinWidth(200), GUILayout.ExpandWidth(true));
                        GUILayout.Label(scale.ToString("F1"));
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("App Scale: ");
                        appScale = GUILayout.HorizontalSlider(appScale, .5f, 2.0f, GUILayout.MinWidth(200), GUILayout.ExpandWidth(true));
                        GUILayout.Label(appScale.ToString("F2"));
                    }

                    SetResolution(GameSettings.SCREEN_RESOLUTION_WIDTH, GameSettings.SCREEN_RESOLUTION_HEIGHT,
                        scale, appScale, GameSettings.FULLSCREEN);
                }

                using (new GUILayout.HorizontalScope())
                {
                    fullScreen = GUILayout.Toggle(fullScreen, "Fullscreen");
                    GUILayout.FlexibleSpace();
                    HighLogic.CurrentGame.Parameters.CustomParams<AR>().useUIScale =
                        GUILayout.Toggle(HighLogic.CurrentGame.Parameters.CustomParams<AR>().useUIScale, "Enable UI Scale");
                    GUILayout.FlexibleSpace();
                }
                if (GUILayout.Button("Set Resolution/Scale"))
                {

                    if (xString != null && yString != null) // && sString != null)
                    {

                        x = Convert.ToInt32(xString);
                        y = Convert.ToInt32(yString);

                        if (x > 0 && y > 0) // && s > 0)
                        {

                            GameSettings.SCREEN_RESOLUTION_HEIGHT = y;
                            GameSettings.SCREEN_RESOLUTION_WIDTH = x;
                            GameSettings.UI_SCALE = scale;
                            GameSettings.UI_SCALE_APPS = appScale;
                            GameSettings.FULLSCREEN = fullScreen;
                            GameSettings.SaveSettings();
                            GameSettings.ApplySettings();

                            SetResolution(GameSettings.SCREEN_RESOLUTION_WIDTH, GameSettings.SCREEN_RESOLUTION_HEIGHT,
                               GameSettings.UI_SCALE, GameSettings.UI_SCALE_APPS, GameSettings.FULLSCREEN);

                            SaveDataConfig(x, y, scale, appScale, fullScreen);

                            Debug.Log("[AnyRes] Set screen resolution");

                            xString = GameSettings.SCREEN_RESOLUTION_WIDTH.ToString();
                            yString = GameSettings.SCREEN_RESOLUTION_HEIGHT.ToString();
                            scale = GameSettings.UI_SCALE;
                            appScale = GameSettings.UI_SCALE_APPS;
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
                if (GUILayout.Button("Save as: " + nameString))
                {
                    var newName = nameString;
                    var newX = xString;
                    var newY = yString;
                    var newFullscreen = fullScreen;

                    SaveConfig(newName, newX, newY, scale.ToString(), appScale.ToString(), newFullscreen);
                    ScreenMessages.PostScreenMessage("Preset saved.  You can change the preset later by using the same name in this editor.", 5, ScreenMessageStyle.UPPER_CENTER);
                    resConfigs = UpdateFilesList();

                }

                if (nameString != HighLogic.LoadedScene + "")
                {
                    if (GUILayout.Button("Save as: " + HighLogic.LoadedScene))
                    {
                        var newName = HighLogic.LoadedScene + "";
                        var newX = xString;
                        var newY = yString;
                        var newFullscreen = fullScreen;

                        SaveConfig(newName, newX, newY, scale.ToString(), appScale.ToString(), newFullscreen);
                        ScreenMessages.PostScreenMessage("Preset saved to current scene: " + HighLogic.LoadedScene, 5, ScreenMessageStyle.UPPER_CENTER);
                        resConfigs = UpdateFilesList();
                    }
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
#if DEBUG
                            Debug.Log("[AnyRes] Set screen resolution from preset");
#endif
                            nameString = GUILayout.TextField(resConfigs[i].node.GetValue("name"));
                            xString = GameSettings.SCREEN_RESOLUTION_WIDTH.ToString();
                            yString = GameSettings.SCREEN_RESOLUTION_HEIGHT.ToString();
                            fullScreen = GameSettings.FULLSCREEN;
                            SetResolution(GameSettings.SCREEN_RESOLUTION_WIDTH, GameSettings.SCREEN_RESOLUTION_HEIGHT,
                                GameSettings.UI_SCALE, GameSettings.UI_SCALE_APPS, GameSettings.FULLSCREEN);

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
            if (sVal == 0)
                sVal = 1.0f;

            float sAppVal;
            float.TryParse(config.GetValue("appScale"), out sAppVal);
            if (sAppVal == 0)
                sAppVal = 1.0f;

            GameSettings.SCREEN_RESOLUTION_HEIGHT = yVal;
            GameSettings.SCREEN_RESOLUTION_WIDTH = xVal;
            GameSettings.UI_SCALE = sVal;
            GameSettings.UI_SCALE_APPS = sAppVal;
            GameSettings.FULLSCREEN = fullscreen;

            // Is this really needed, should be applied by the ApplySettings below
            //SetResolution(xVal, yVal, sVal, sAppVal, fullscreen);
            GameSettings.SaveSettings();
            GameSettings.ApplySettings();

            if (saveConfig)
            {
                SaveDataConfig(xVal, yVal, sVal, sAppVal, fullscreen);

            }
        }

        static void SaveDataConfig(int xVal, int yVal, float sVal, float appScale, bool fullscreen)
        {
            SaveConfig(LASTSETRES, xVal.ToString(), yVal.ToString(), sVal.ToString(), appScale.ToString(), fullscreen);
            var resConfigs = UpdateFilesList(true);
            if (resConfigs.Length == 1)
            {
                SetInitialRes.LastSetRes = resConfigs[0].node;

            }

        }

        static void SaveConfig(string newName, string newX, string newY, string newS, string newAppScale, bool newFullscreen)
        {
            ConfigNode config = new ConfigNode(newName);
            config.AddValue("name", newName);
            config.AddValue("x", newX);
            config.AddValue("y", newY);
            if (newS == "0")
                newS = "1";
            config.AddValue("scale", newS);
            config.AddValue("appScale", newAppScale);
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
                if (sVal > highestUIscale)
                {
                    highestUIscale = sVal;
                }
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

