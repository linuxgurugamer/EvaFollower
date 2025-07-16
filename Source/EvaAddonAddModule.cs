using UnityEngine;

namespace MSD.EvaFollower
{
    /// <summary>
    /// Add the module to all kerbals available. 
    /// </summary>
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class EvaAddonAddModule : MonoBehaviour
    {
        public void Awake()
        {
            #region NO_LOCALIZATION
            EvaDebug.DebugLog("Loaded AddonAddModule.");

            var parts = new string[]
            {
              "kerbalEVA",
              "kerbalEVAVintage",
              "kerbalEVAfemale",
              "kerbalEVAfemaleVintage",
              "kerbalEVA_RD_Exp",
              "kerbalEVA_female_Exp",
              "kerbalEVA_RD_Future",
              "kerbalEVA_female_Future",
              "kerbalEVAfemaleFuture",
              "maleEVA",
              "femaleEVA",

              "kerbalEVASlimSuitFemale",
              "kerbalEVASlimSuit",
              "kerbalEVAFuture",
              "kerbalFemaleFuture",

            };

            foreach (var part in parts)
            {
                var EVA = new ConfigNode("MODULE");
                EVA.AddValue("name", "EvaModule");
                try
                {
                    PartLoader.getPartInfoByName(part).partPrefab.AddModule(EVA);
                }
                catch { }
            }
        }
    }
}
#endregion