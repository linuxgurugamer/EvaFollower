using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using EvaFuel;

namespace MSD.EvaFollower
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class EVAFuelGlobals : MonoBehaviour
    {
        public static bool ChangeEVAPropellent;
        void Start()
        {
            ChangeEVAPropellent = false;
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

        private Rect settingsRect = new Rect(200, 200, 275, 400);

        int curResIndex = -1;
        Vector2 scrollPosition1 = Vector2.zero;
        static List<PartResourceDefinition> allResources = null;
        static List<string> allResourcesDisplayNames = null;
        static List<string> fuelResources = null;
        static List<string> bannedResources = null;

        public string selectedFuel;


        GUIStyle smallButtonStyle, smallScrollBar;

        public static readonly String ROOT_PATH = KSPUtil.ApplicationRootPath;
        public static string MOD = null;
        static string EVA_FUELRESOURCES = "FUELRESOURCES";
        static string BANNED_RESOURCES = "BANNED";
        public static String EVAFUEL_NODE = MOD;


        void Start()
        {
            Debug.Log("SelectEVAFuelType.Start");

            Instance = this;
            smallButtonStyle = new GUIStyle(HighLogic.Skin.button);
            smallButtonStyle.stretchHeight = false;
            smallButtonStyle.fixedHeight = 20f;

            smallScrollBar = new GUIStyle(HighLogic.Skin.verticalScrollbar);
            smallScrollBar.fixedWidth = 8f;

            MOD = Assembly.GetAssembly(typeof(EvaFuelManager)).GetName().Name;
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
            return "xxx";
        }
        public void Draw()
        {
            if (string.IsNullOrEmpty(MOD))
                return;

            Debug.Log("SelectEVAFuelType.Draw");

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


        public List<String> getFuelResources(bool banned = false)
        {
            List<string> fr = new List<String>();
            if (string.IsNullOrEmpty(MOD))
            {
              return fr;
            }

            string fname = ROOT_PATH + "GameData/" + MOD + "/PluginData/fuelResources.cfg";

            var configFile = ConfigNode.Load(fname);
            if (configFile != null)
            {
              var configFileNode = configFile.GetNode(EVAFUEL_NODE);

              if (configFileNode != null)
                {
                  ConfigNode configDataNode;
                  configDataNode = configFileNode.GetNode(banned ? BANNED_RESOURCES : EVA_FUELRESOURCES);

                  if (configDataNode != null)
                  {
                    fr = configDataNode.GetValuesList("resource");
                  }
                }
                else
                  Debug.LogError("NODENAME not found: " + EVAFUEL_NODE);
            }
            else
              Debug.LogError("File not found: " + fname);

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
                        if (ar.name == HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName)
                            curResIndex = cnt;
                        cnt++;

                    }
                    catch
                    {
                        Log.Error("Can't find resource: " + s + " in allResources");
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
                    if (ar.name == HighLogic.CurrentGame.Parameters.CustomParams<EVAFuelSettings>().ShipPropellantName)
                        curResIndex = cnt;
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
            GUILayout.Label("Select EVA Propellent from list below");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            var newallRes = GUILayout.Toggle(allRes, "All resources");
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            var newfuelRes = GUILayout.Toggle(fuelRes, "Fuel resources");
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
            if (GUILayout.Button("OK"))
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
            if (GUILayout.Button("Cancel"))
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
        public override string Section { get { return "EVA Follower"; } }
        public override string DisplaySection { get { return "EVA Follower"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }


        [GameParameters.CustomParameterUI("Show Debug Lines?")]
        public bool DisplayDebugLinesSetting = false;

        [GameParameters.CustomParameterUI("Show Loading Kerbals?")]
        public bool DisplayLoadingKerbals = true;

        [GameParameters.CustomParameterUI("Enable Helmet Toggle?")]
        public bool DisplayToggleHelmet = false;

        [GameParameters.CustomParameterUI("Target Vessel By Selection?")]
        public bool TargetVesselBySelection = true;


        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {

        }
        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            return !EVAFuelGlobals.ChangeEVAPropellent;
        }
    }
}

