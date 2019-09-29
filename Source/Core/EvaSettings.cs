using System;
using System.Collections.Generic;
using System.IO;
using MSD.EvaFollower.ControlTypes;
using UnityEngine;
using TextReader = KSP.IO.TextReader;
using TextWriter = KSP.IO.TextWriter;

namespace MSD.EvaFollower
{
  internal class EvaSettings
  {
    public static readonly string RootPath = KSPUtil.ApplicationRootPath;
    private static readonly string _configBaseFolder = RootPath + "GameData/";
    private static readonly string _baseFolder = _configBaseFolder + "EvaFollower/";
    private static readonly string _nodename = "EvaFollower";
    private static string _cfgFile = _baseFolder + "PluginData/Settings.cfg";


    internal static bool DisplayDebugLines = false;
    internal static int SelectMouseButton;
    internal static int DispatchMouseButton = 2;

    internal static string SelectKeyButton = "o";
    internal static string DispatchKeyButton = "p";
    
    private static readonly Dictionary<Guid, string> _collection = new Dictionary<Guid, string>();

    private static bool _isLoaded;

    private static string ConfigFileName
    {
      get { return _baseFolder + "/PluginData/Config.cfg"; }
    }

    public static void LoadConfiguration()
    {
      if (File.Exists(ConfigFileName))
      {
        var node = ConfigNode.Load(ConfigFileName);
        var data = node?.GetNode(_nodename);

        if (data != null)
        {
          node.TryGetValue("selectMouseButton", ref SelectMouseButton);
          node.TryGetValue("dispatchMouseButton", ref DispatchMouseButton);
          node.TryGetValue("selectKeyButton", ref SelectKeyButton);
          node.TryGetValue("dispatchKeyButton", ref DispatchKeyButton);
        }
      }
    }

    public static void SaveConfiguration()
    {
      EvaDebug.DebugWarning("SaveConfiguration()");

      var node = new ConfigNode();
      var data = new ConfigNode();
      
      data.AddValue("selectMouseButton", SelectMouseButton);
      data.AddValue("dispatchMouseButton", DispatchMouseButton);
      data.AddValue("selectMouseButton", SelectMouseButton);
      data.AddValue("dispatchKeyButton", DispatchKeyButton);

      node.AddNode(_nodename, data);

      Debug.Log("Saving to: " + ConfigFileName);
      node.Save(ConfigFileName);
    }

    public static bool FileExcist(string name)
    {
      return KSP.IO.File.Exists<EvaSettings>(name);
    }

    public static void Load()
    {
      EvaDebug.DebugWarning("OnLoad()");
      if (HighLogic.CurrentGame.Parameters.CustomParams<EvaFollowerMiscSettings>().DisplayLoadingKerbals)
      {
        ScreenMessages.PostScreenMessage("Loading Kerbals...", 3, ScreenMessageStyle.LOWER_CENTER);
      }

      LoadFunction();
    }

    public static void LoadFunction()
    {
      EvaDebug.ProfileStart();
      LoadFile();
      EvaDebug.ProfileEnd("EvaSettings.Load()");
      _isLoaded = true;
    }

    public static void Save()
    {
      if (_isLoaded)
      {
        EvaDebug.DebugWarning("OnSave()");

        if (HighLogic.CurrentGame.Parameters.CustomParams<EvaFollowerMiscSettings>().DisplayLoadingKerbals)
        {
          ScreenMessages.PostScreenMessage("Saving Kerbals...", 3, ScreenMessageStyle.LOWER_CENTER);
        }

        SaveFunction();

        _isLoaded = false;
      }
    }

    public static void SaveFunction()
    {
      EvaDebug.ProfileStart();
      SaveFile();
      EvaDebug.ProfileEnd("EvaSettings.Save()");
    }

    public static void LoadEva(EvaContainer container)
    {
      EvaDebug.DebugWarning("EvaSettings.LoadEva(" + container.Name + ")");

      //The eva was already has a old save.
      //Load it.
      if (_collection.ContainsKey(container.FlightId))
      {
        //string evaString = collection[container.flightID];
        //EvaDebug.DebugWarning(evaString);

        container.FromSave(_collection[container.FlightId]);
      }
    }

    public static void SaveEva(EvaContainer container)
    {
      EvaDebug.DebugWarning("EvaSettings.SaveEva(" + container.Name + ")");

      if (container.Status == Status.Removed)
      {
        if (_collection.ContainsKey(container.FlightId))
        {
          _collection.Remove(container.FlightId);
        }
      }
      else
      {
        //The eva was already has a old save.
        if (_collection.ContainsKey(container.FlightId))
        {
          //Replace the old save.
          _collection[container.FlightId] = container.ToSave();
        }
        else
        {
          //No save yet. Add it now.
          _collection.Add(container.FlightId, container.ToSave());
        }
      }
    }

    private static void LoadFile()
    {
      var fileName = string.Format("Evas-{0}.txt", HighLogic.CurrentGame.Title);
      if (FileExcist(fileName))
      {
        var tr = TextReader.CreateForType<EvaSettings>(fileName);

        var file = tr.ReadToEnd();
        tr.Close();

        var reader = new EvaTokenReader(file);

        EvaDebug.DebugLog("Size KeySize: " + _collection.Count);

        //read every eva.
        while (!reader.EOF)
        {
          //Load all the eva's in the list.
          LoadEva(reader.NextToken('[', ']'));
        }
      }
    }

    private static void LoadEva(string eva)
    {
      var flightId = GetFlightIDFromEvaString(eva);
      _collection.Add(flightId, eva);
    }


    private static Guid GetFlightIDFromEvaString(string evaString)
    {
      var reader = new EvaTokenReader(evaString);

      var sflightId = reader.NextTokenEnd(',');

      //Load the eva
      var flightId = new Guid(sflightId);
      return flightId;
    }


    private static void SaveFile()
    {
      var tw = TextWriter.CreateForType<EvaSettings>(string.Format("Evas-{0}.txt", HighLogic.CurrentGame.Title));

      foreach (var item in _collection)
      {
        tw.Write("[" + item.Value + "]");
      }

      tw.Close();

      _collection.Clear();
    }
  }
}