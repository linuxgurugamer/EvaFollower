using UnityEngine;

namespace MSD.EvaFollower
{
  /// <summary>
  ///   Add the module to all kerbals available.
  /// </summary>
  [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
  internal class EvaAddonAddModule : MonoBehaviour
  {
    public void Awake()
    {
      EvaDebug.DebugLog("Loaded AddonAddModule.");

      TryAttachPartModule("kerbalEVA");
      TryAttachPartModule("kerbalEVAVintage");
      TryAttachPartModule("kerbalEVAfemale");
      TryAttachPartModule("kerbalEVAfemaleVintage");
    }

    private static void TryAttachPartModule(string partName)
    {
      ConfigNode eva = new ConfigNode("MODULE");
      eva.AddValue("name", "EvaModule");
      try
      {
        PartLoader.getPartInfoByName(partName).partPrefab.AddModule(eva);
      }
      catch
      {
        Debug.LogWarning($"Failed addomg module to {partName}.");
      }
    }
  }
}