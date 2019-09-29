using System;
using UnityEngine;
using Object = UnityEngine.Object;
using SD = System.Diagnostics;

namespace MSD.EvaFollower
{
#if DEBUG
  [KSPAddon(KSPAddon.Startup.MainMenu, true)]
  public class EvaDebug : MonoBehaviour
  {
    private Rect _pos;
    private string _content = "None";
    private GUIStyle _style;

    public void Start()
    {
      DontDestroyOnLoad(this);
    }

    public void OnGUI()
    {
      if (HighLogic.LoadedScene == GameScenes.FLIGHT)
      {
        if (_style == null)
        {
          var w = 600;
          var h = 250;

          _pos = new Rect(Screen.width - (20 + w), 60, w, h);

          _style = new GUIStyle(GUI.skin.label);
          _style.alignment = TextAnchor.UpperRight;
          _style.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 0.6f);
        }

        GUI.Label(_pos, _content, _style);
      }
    }

    public void Update()
    {
      if (HighLogic.LoadedScene == GameScenes.FLIGHT)
      {
        _content = "Active Kerbals: " + EvaController.Instance.Collection.Count;
        _content += Environment.NewLine + EvaController.Instance.Debug;
      }
      else
      {
        _content = "None";
      }
    }
#else
          public class EvaDebug : MonoBehaviour
        {
#endif
    //Debug log yes/no
    private static readonly bool _debugLogActive = true;

    public static void DebugLog(string text)
    {
      if (_debugLogActive)
      {
        Debug.Log("[EFX] " + text);
      }
    }

    public static void DebugLog(string text, Object context)
    {
      if (_debugLogActive)
      {
        Debug.Log("[EFX] " + text, context);
      }
    }

    public static void DebugWarning(string text)
    {
      if (_debugLogActive)
      {
        Debug.LogWarning("[EFX] " + text);
      }
    }

    public static void DebugError(string text)
    {
      if (_debugLogActive)
      {
        Debug.LogError("[EFX] " + text);
      }
    }


    public static void ProfileStart()
    {
      StartTimer();
    }

    public static void ProfileEnd(string name)
    {
      EndTimer();
      DebugWarning(string.Format("Profile: {0}: {1}ms", name, Elapsed));
    }

    public static float Elapsed;
    private static SD.Stopwatch _watch;

    /// <summary>
    ///   Start the timer
    /// </summary>
    private static void StartTimer()
    {
      _watch = SD.Stopwatch.StartNew();
    }

    /// <summary>
    ///   End the timer, and get the elapsed time.
    /// </summary>
    private static void EndTimer()
    {
      _watch.Stop();
      Elapsed = _watch.ElapsedMilliseconds;
    }
  }
}