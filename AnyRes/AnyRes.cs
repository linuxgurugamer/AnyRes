using KSP.Localization;
using KSP.UI.Screens;
using KSP.UI.Screens.Settings.Controls;
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

        public static Rect anyresWinRect = new Rect(35, 99, 420, 380);
        public Rect deleteRect = new Rect((Screen.width - 200) / 2, (Screen.height - 100) / 2, 200, 100);

        #region NO_LOCALIZATION
        public string nameString = "";
        public string xString = "1280";
        public string yString = "720";

        public int x = 1280;
        public int y = 720;

        const string LASTSETRES = "LastSetRes";

        public bool windowEnabled = false;
        public bool fullScreen = true;
        public bool reloadScene = false;

        static ToolbarControl toolbarControl = null;

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


            xString = GameSettings.SCREEN_RESOLUTION_WIDTH.ToString();
            yString = GameSettings.SCREEN_RESOLUTION_HEIGHT.ToString();
            x = GameSettings.SCREEN_RESOLUTION_WIDTH;
            y = GameSettings.SCREEN_RESOLUTION_HEIGHT;

            fullScreen = GameSettings.FULLSCREEN;

            resConfigs = UpdateFilesList();

            if (toolbarControl == null)
            {
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
        }

        internal const string MODID = "AnyRes_NS";
        internal const string MODNAME = "AnyRes";
        internal float uiScale = 1.0f;
        void OnTrue()
        {
            windowEnabled = true;
            uiScale = GameSettings.UI_SCALE;
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
                #endregion
                anyresWinRect = ClickThruBlocker.GUIWindow(09271, anyresWinRect, GUIActive, Localizer.Format("#LOC_AnyRes_AnyRes"));

            }
            if (confirmDeleteEnabled)
                deleteRect = ClickThruBlocker.GUIWindow(09276, deleteRect, ConfirmDelete, Localizer.Format("#LOC_AnyRes_Confirm"));

            if (Screen.width != GameSettings.SCREEN_RESOLUTION_WIDTH || Screen.height != GameSettings.SCREEN_RESOLUTION_HEIGHT)
            {
                Log.Info("Resetting Video Resolution to: " + GameSettings.SCREEN_RESOLUTION_WIDTH + " x " + GameSettings.SCREEN_RESOLUTION_HEIGHT +
                  ", " + // NO_LOCALIZATION
                    (GameSettings.FULLSCREEN ? Localizer.Format("#LOC_AnyRes_Fullscreen") : Localizer.Format("#LOC_AnyRes_Windowed")));
                Screen.SetResolution(GameSettings.SCREEN_RESOLUTION_WIDTH, GameSettings.SCREEN_RESOLUTION_HEIGHT, GameSettings.FULLSCREEN);
            }
        }

        void SetScreenResolution()
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
                    GameSettings.ApplySettings();
                    Screen.SetResolution(x, y, fullScreen);

                    SaveDataConfig(x, y, fullScreen);


                    Debug.Log("[AnyRes] Set screen resolution");
                }
                else
                {
                    ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_AnyRes_One_or_both_of_your_value"), 1, ScreenMessageStyle.UPPER_CENTER);
                }
            }
            else
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_AnyRes_The_values_you_have_set_a"), 1, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        void GUIActive(int windowID)
        {
            GUILayout.BeginHorizontal();

            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(Localizer.Format("#LOC_AnyRes_Name"));
                    nameString = GUILayout.TextField(nameString);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(Localizer.Format("#LOC_AnyRes_Width"));
                    xString = GUILayout.TextField(xString);
                    xString = Regex.Replace(xString, @"[^0-9]", "");
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(Localizer.Format("#LOC_AnyRes_Height"));
                    yString = GUILayout.TextField(yString);
                    yString = Regex.Replace(yString, @"[^0-9]", "");
                }
                fullScreen = GUILayout.Toggle(fullScreen, Localizer.Format("#LOC_AnyRes_Fullscreen"));
                //			reloadScene = GUILayout.Toggle (reloadScene, "Reload scene");
                if (GUILayout.Button(Localizer.Format("#LOC_AnyRes_Set_Screen_Resolution")))
                {
                    SetScreenResolution();
                }
                if (nameString == "")
                    GUI.enabled = false;
                if (GUILayout.Button(Localizer.Format("#LOC_AnyRes_Save")))
                {
                    var newName = nameString;
                    var newX = xString;
                    var newY = yString;
                    var newFullscreen = fullScreen;

                    SaveConfig(newName, newX, newY, newFullscreen);
                    ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_AnyRes_Preset_saved_You_can_chan"), 5, ScreenMessageStyle.UPPER_CENTER);
                    resConfigs = UpdateFilesList();

                }


                if (resConfigs.Length == 0)
                    GUI.enabled = false;
                else
                    GUI.enabled = true;
                if (deleteEnabled)
                {
                    if (GUILayout.Button(Localizer.Format("#LOC_AnyRes_Disable_Delete")))
                    {
                        deleteEnabled = false;
                    }

                }
                else
                {
                    if (GUILayout.Button(Localizer.Format("#LOC_AnyRes_Enable_Delete")))
                    {
                        deleteEnabled = true;
                    }
                }

                if (HighLogic.CurrentGame.Parameters.CustomParams<AR>().saveWinPos)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(Localizer.Format("#LOC_AnyRes_Save_Win_Pos")))
                        {
                            SaveWinPos();
                        }

                        if (GUILayout.Button(Localizer.Format("#LOC_AnyRes_Clear_Win_Pos")))
                        {
                            SaveWinPos(clear: true);
                        }
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(Localizer.Format("#LOC_AnyRes_UI_Scale") + "(" + (Mathf.Round(uiScale * 10f) * 10).ToString("F0") + ")", GUILayout.Width(90));
                    uiScale = GUILayout.HorizontalSlider(uiScale, 0.8f, 2.0f, GUILayout.Width(100));
                    if (GUILayout.Button("Apply"))
                    {
                        uiScale = Mathf.Round(uiScale * 10f) / 10f;
                        float percentage = uiScale / GameSettings.UI_SCALE;
                        Debug.Log("[AnyRes] UI Scale percentage: " + percentage);
                        GameSettings.UI_SCALE_CREW = GameSettings.UI_SCALE_CREW * percentage;
                        GameSettings.UI_SCALE_NAVBALL = GameSettings.UI_SCALE_NAVBALL * percentage;
                        GameSettings.UI_SCALE_MODE = GameSettings.UI_SCALE_MODE * percentage;
                        GameSettings.UI_SCALE_STAGINGSTACK = GameSettings.UI_SCALE_STAGINGSTACK * percentage;
                        GameSettings.UI_SCALE_APPS = GameSettings.UI_SCALE_APPS * percentage;
                        GameSettings.UI_SCALE_MAPOPTIONS = GameSettings.UI_SCALE_MAPOPTIONS * percentage;
                        GameSettings.UI_SCALE_ALTIMETER = GameSettings.UI_SCALE_ALTIMETER * percentage;
                        GameSettings.UI_SCALE_TIME = GameSettings.UI_SCALE_TIME * percentage;


                        GameSettings.UI_SCALE = uiScale;
                        SettingsLayoutConfig.SaveChanges();
                        SettingsControlBase[] componentsInChildren = GetComponentsInChildren<SettingsControlBase>(includeInactive: true);
                        int i = 0;
                        for (int num = componentsInChildren.Length; i < num; i++)
                        {
                            componentsInChildren[i].OnApply();
                        }
                        GameSettings.SaveSettings();
                        GameSettings.ApplySettings();
                        GameEvents.OnGameSettingsApplied.Fire();

                        //UIMasterController.Instance.SetScale(uiScale);
                        //UIMasterController.Instance.SetAppScale(GameSettings.UI_SCALE_APPS);

                        SaveDataConfig(x, y, GameSettings.FULLSCREEN);

                        SetScreenResolution();
                        uiScale = GameSettings.UI_SCALE;


                    }
                }

                if (GUILayout.Button(Localizer.Format("#LOC_AnyRes_Close")))
                {
                    toolbarControl.SetFalse(true);
                }
            }

            using (new GUILayout.VerticalScope())
            {
                scrollViewPos = GUILayout.BeginScrollView(scrollViewPos, GUILayout.Width(150));
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
                SetInitialRes.LastSetRes = files[0].node;

            }
        }

        #region NO_LOCALIZATION
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
        #endregion

        void ConfirmDelete(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(Localizer.Format("#LOC_AnyRes_Confirm_delete_of") + deleteFileName);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Localizer.Format("#LOC_AnyRes_Cancel")))
            {
                deleteEnabled = false;
                confirmDeleteEnabled = false;
            }
            if (GUILayout.Button(Localizer.Format("#LOC_AnyRes_Yes")))
            {
                //deleteEnabled = false;
                confirmDeleteEnabled = false;
                System.IO.File.Delete(deleteFile);
                resConfigs = UpdateFilesList();
            }
            GUILayout.EndHorizontal();
        }

        #region NO_LOCALIZATION
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
            }

            return flist.ToArray();
        }
        #endregion

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

