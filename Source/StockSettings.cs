//using EvaFuel;
using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using KSP_Log;

namespace MSD.EvaFollower
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class EVAFuelGlobals : MonoBehaviour
    {
        public static bool changeEVAPropellent;
        static public Log Log;

        void Start()
        {
            changeEVAPropellent = false;
            if (!modsFound)
            {
                buildModList();
                modsFound = true;
            }
#if DEBUG
            Log = new Log("EVAFollower", Log.LEVEL.INFO);
#else
            Log = new Log("EVAFollower", Log.LEVEL.ERROR);
#endif

        }

        bool modsFound = false;
        static List<String> installedMods = new List<String>();
        void buildModList()
        {
            //https://github.com/Xaiier/Kreeper/blob/master/Kreeper/Kreeper.cs#L92-L94 <- Thanks Xaiier!
            foreach (AssemblyLoader.LoadedAssembly a in AssemblyLoader.loadedAssemblies)
            {
                installedMods.Add(a.name);
            }
        }
        static public bool hasMod(string modIdent)
        {
            return installedMods.Contains(modIdent);
        }

    }

    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    class SelectEVAFuelType : MonoBehaviour
    {
        public static SelectEVAFuelType Instance;

        public enum Answer
        {
            inActive,
            notAnswered,
            cancel,
            answered
        };
        public Answer answer = Answer.inActive;
        public float lastTimeTic = 0;

        private Rect settingsRect;
        int curResIndex = -1;
        Vector2 scrollPosition1 = Vector2.zero;
        static List<PartResourceDefinition> allResources = null;
        static List<string> allResourcesDisplayNames = null;
        static List<string> fuelResources = null;
        static List<string> bannedResources = null;

        public string selectedFuel;


        GUIStyle smallButtonStyle, smallScrollBar;

        public static String ROOT_PATH;
        public static string MOD = null;
        static string EVA_FUELRESOURCES = "FUELRESOURCES"; // NO_LOCALIZATION
        static string BANNED_RESOURCES = "BANNED"; // NO_LOCALIZATION
        public static String EVAFUEL_NODE = MOD;


        void Start()
        {
            EVAFuelGlobals.Log.Info("SelectEVAFuelType.Start"); // NO_LOCALIZATION
            Instance = this;
            smallButtonStyle = new GUIStyle(HighLogic.Skin.button);
            smallButtonStyle.stretchHeight = false;
            smallButtonStyle.fixedHeight = 20f;

            smallScrollBar = new GUIStyle(HighLogic.Skin.verticalScrollbar);
            smallScrollBar.fixedWidth = 8f;

            // The follopwing
            if (EVAFuelGlobals.hasMod("EvaFuel")) // NO_LOCALIZATION
                MOD = GetEvaFuel();
            else
                MOD = null;
            settingsRect = new Rect(200, 200, 275, 400);
            ROOT_PATH = KSPUtil.ApplicationRootPath;

        }

        string GetEvaFuel()
        {
            return "";
            //return  Assembly.GetAssembly(typeof(EvaFuelManager)).GetName().Name;
        }
        void OnGUI()
        {
            if (answer == Answer.inActive)
                return;
            if (Time.realtimeSinceStartup - lastTimeTic > 0.25)
            {
                answer = Answer.inActive;
                return;
            }
            Draw();
        }

        string setLabel()
        {
            return Localizer.Format("#LOC_EVAFollower_4");
        }
        public void Draw()
        {
            if (MOD == null || MOD == "")
                return;
            #region NO_LOCALIZATION
            EVAFuelGlobals.Log.Info("SelectEVAFuelType.Draw");
            if (allResources == null)
                getAllResources();

            // The settings are only available in the space center
            GUI.skin = HighLogic.Skin;
            settingsRect = GUILayout.Window("EVAFuelSettings".GetHashCode(),
                                            settingsRect,
                                            SettingsWindowFcn,
                                            "EVA Follower Settings",
                                            GUILayout.ExpandWidth(true),
                                            GUILayout.ExpandHeight(true));
        }
        #endregion

        public List<String> getFuelResources(bool banned = false)
        {
            List<string> fr = new List<String>();
            if (MOD == null || MOD == "")
                return fr;
            ConfigNode configFile = new ConfigNode();
            ConfigNode configFileNode = new ConfigNode();
            ConfigNode configDataNode;
            
            string fname = ROOT_PATH + "GameData/" + MOD + "/PluginData/fuelResources.cfg"; // NO_LOCALIZATION

            configFile = ConfigNode.Load(fname);
            if (configFile != null)
            {
                configFileNode = configFile.GetNode(EVAFUEL_NODE);

                if (configFileNode != null)
                {
                    if (banned)
                        configDataNode = configFileNode.GetNode(BANNED_RESOURCES);
                    else
                        configDataNode = configFileNode.GetNode(EVA_FUELRESOURCES);
                    if (configDataNode != null)
                        fr = configDataNode.GetValuesList("resource"); // NO_LOCALIZATION
                }
                else
                    EVAFuelGlobals.Log.Error("NODENAME not found: " + EVAFUEL_NODE); // NO_LOCALIZATION
            }
            else
                EVAFuelGlobals.Log.Error("File not found: " + fname); // NO_LOCALIZATION

            return fr;
        }


        void fillResourceDisplayNames()
        {
            if (fuelResources == null || allResources == null)
                getAllResources();
            allResourcesDisplayNames = new List<string>();
            int cnt = 0;
            if (fuelRes && fuelResources.Count > 0)
            {

                foreach (var s in fuelResources)
                {
                    try
                    {
                        var ar = allResources.Find(o => o.name == s);
                        if (ar.displayName != null)
                            allResourcesDisplayNames.Add(ar.displayName);
                        else
                            allResourcesDisplayNames.Add(ar.name);
                        //if (ar.name == HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName)
                        //    curResIndex = cnt;
                        cnt++;

                    }
                    catch
                    {
                        EVAFuelGlobals.Log.Error("Can't find resource: " + s + " in allResources"); // NO_LOCALIZATION
                    }
                }
            }
            else
            {
                foreach (var ar in allResources)
                {
                    if (bannedResources.Contains(ar.name))
                        continue;
                    if (ar.displayName != null)
                        allResourcesDisplayNames.Add(ar.displayName);
                    else
                        allResourcesDisplayNames.Add(ar.name);
                    //if (ar.name == HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName)
                    //    curResIndex = cnt;
                    cnt++;
                }

            }
        }

        void getAllResources()
        {
            allResources = new List<PartResourceDefinition>();

            foreach (PartResourceDefinition rs in PartResourceLibrary.Instance.resourceDefinitions)
            {
                allResources.Add(rs);
            }
            allResources = allResources.OrderBy(o => o.displayName).ToList();
            fuelResources = getFuelResources();
            bannedResources = getFuelResources(true);

            fillResourceDisplayNames();

        }
        bool allRes = false;
        bool fuelRes = true;
        void SettingsWindowFcn(int windowID)
        {

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(Localizer.Format("#LOC_EVAFollower_5"));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            var newallRes = GUILayout.Toggle(allRes, Localizer.Format("#LOC_EVAFollower_6"));
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            var newfuelRes = GUILayout.Toggle(fuelRes, Localizer.Format("#LOC_EVAFollower_7"));
            GUILayout.EndVertical();
            if (newfuelRes && allRes)
            {
                allRes = false;
                fuelRes = true;
                fillResourceDisplayNames();
            }
            else
                if (newallRes & fuelRes)
            {
                allRes = true;
                fuelRes = false;
                fillResourceDisplayNames();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1);

            curResIndex = GUILayout.SelectionGrid(curResIndex, allResourcesDisplayNames.ToArray(), 1, smallButtonStyle);

            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Localizer.Format("#LOC_EVAFollower_8")))
            {
                answer = Answer.answered;
                if (allRes)
                {
                    selectedFuel = allResources[curResIndex].name;
                }
                else
                {
                    int cnt = 0;
                    for (int i = 0; i < fuelResources.Count; i++)
                    {
                        try
                        {
                            var ar = allResources.Find(o => fuelResources[i] == o.name);

                            if (cnt == curResIndex)
                            {
                                selectedFuel = ar.name;
                                break;
                            }
                            cnt++;
                        }
                        catch
                        { }
                    }
                }
            }
            if (GUILayout.Button(Localizer.Format("#LOC_EVAFollower_9")))
                answer = Answer.cancel;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            // This call allows the user to drag the window around the screen
            GUI.DragWindow();
        }
    }



    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings
    // HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>()

    public class EvaFollowerMiscSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return ""; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return Localizer.Format("#LOC_EVAFollower_10"); } }
        public override string DisplaySection { get { return Localizer.Format("#LOC_EVAFollower_10"); } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }


        [GameParameters.CustomParameterUI("Show Debug Lines?")]
        public bool displayDebugLinesSetting = false;

        [GameParameters.CustomParameterUI("Show Loading Kerbals?")]
        public bool displayLoadingKerbals = true;

        [GameParameters.CustomParameterUI("Enable Helmet Toggle?")]
        public bool displayToggleHelmet = false;

        [GameParameters.CustomParameterUI("Target Vessel By Selection?")]
        public bool targetVesselBySelection = true;


        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {

        }
        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {

            return !EVAFuelGlobals.changeEVAPropellent;
        }
    }
}

