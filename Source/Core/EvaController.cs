using System;
using System.Collections.Generic;
using MSD.EvaFollower.ControlTypes;
using UnityEngine;

namespace MSD.EvaFollower
{
  [KSPAddon(KSPAddon.Startup.Flight, false)]
  internal class EvaController : MonoBehaviour
  {
    public static EvaController Instance;
    public string Debug = "";
    public List<EvaContainer> Collection = new List<EvaContainer>();

    public void Start()
    {
      EvaDebug.DebugWarning("EvaController.Start()");
      //initialize the singleton.
      Instance = this;

      GameEvents.onPartPack.Add(OnPartPack);
      GameEvents.onPartUnpack.Add(OnPartUnpack);

      GameEvents.onCrewOnEva.Add(OnCrewOnEva);
      GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
//            GameEvents.onCrewKilled.Add(OnCrewKilled);
      GameEvents.onVesselWillDestroy.Add(VesselDestroyed);

      GameEvents.onGameStateSave.Add(OnSave);
      GameEvents.onFlightReady.Add(onFlightReadyCallback);
    }

    public void OnDestroy()
    {
      EvaDebug.DebugWarning("EvaController.OnDestroy()");


      GameEvents.onPartPack.Remove(OnPartPack);
      GameEvents.onPartUnpack.Remove(OnPartUnpack);

      GameEvents.onCrewOnEva.Remove(OnCrewOnEva);
      GameEvents.onCrewBoardVessel.Remove(OnCrewBoardVessel);
//            GameEvents.onCrewKilled.Remove(OnCrewKilled);
      GameEvents.onVesselWillDestroy.Add(VesselDestroyed);

      GameEvents.onGameStateSave.Remove(OnSave);
      GameEvents.onFlightReady.Remove(onFlightReadyCallback);
    }

    public void OnSave(ConfigNode node)
    {
      //Save the eva list.
      // Might be double.
      foreach (var item in Collection)
      {
        EvaSettings.SaveEva(item);
      }

      EvaSettings.Save();
    }

    public void OnPartPack(Part part)
    {
      if (part.vessel.isEVA)
      {
        //save before pack
        EvaDebug.DebugWarning("Pack: " + part.vessel.name);

        Unload(part.vessel, false);
      }
    }

    public void OnPartUnpack(Part part)
    {
      if (part.vessel.isEVA)
      {
        //save before pack
        EvaDebug.DebugWarning("Unpack: " + part.vessel.name);

        Load(part.vessel);
      }
    }

    /// <summary>
    ///   Runs when the kerbal goes on EVA.
    /// </summary>
    /// <param name="e"></param>
    public void OnCrewOnEva(GameEvents.FromToAction<Part, Part> e)
    {
      //add new kerbal
      EvaDebug.DebugLog("OnCrewOnEva()");
      Load(e.to.vessel);
    }

    /// <summary>
    ///   Runs when the EVA goes onboard a vessel.
    /// </summary>
    /// <param name="e"></param>
    public void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> e)
    {
      //remove kerbal
      EvaDebug.DebugLog("OnCrewBoardVessel()");
      Unload(e.from.vessel, true);
    }

    /// <summary>
    ///   Runs when the EVA is killed.
    /// </summary>
    /// <param name="report"></param>
/*
        public void OnCrewKilled(EventReport report)
        {
            EvaDebug.DebugLog("OnCrewKilled()");
		KerbalRoster boboo = new KerbalRoster(Game.Modes.SANDBOX);	
		print(boboo[report.sender].name);
		//MonoBehaviour.print(report.origin);
		//MonoBehaviour.print(report.origin.vessel);
            //Unload(report.origin.vessel, true);
        }
*/
    public void VesselDestroyed(Vessel report)
    {
      EvaDebug.DebugLog("VesselDestroyed()");
      if (report.isEVA)
      {
        Unload(report, true);
      }
    }

    public void Load(Vessel vessel)
    {
      if (!vessel.isEVA)
      {
        EvaDebug.DebugWarning("Tried loading a non eva.");
        return;
      }

      var currentEVA = vessel.GetComponent<KerbalEVA>();

      if (!Contains(vessel.id))
      {
        var container = new EvaContainer(vessel.id);

        //load the vessel here.
        container.Load(currentEVA);
        EvaSettings.LoadEva(container);

        Collection.Add(container);
      }
      else
      {
        //Reload
        var container = GetEva(vessel.id);

        container.Load(currentEVA);
        EvaSettings.LoadEva(container);
      }
    }

    public void Unload(Vessel vessel, bool delete)
    {
      if (!vessel.isEVA)
      {
        EvaDebug.DebugWarning("Tried unloading a non eva.");
        return;
      }

      EvaDebug.DebugLog("Unload(" + vessel.name + ")");

      foreach (var item in Collection)
      {
        if (item.FlightId == vessel.id)
        {
          if (delete)
          {
            item.Status = Status.Removed;
          }

          //unload the vessel here. 
          item.Unload();
          EvaSettings.SaveEva(item);


          EvaDebug.DebugLog("Remove EVA: (" + vessel.name + ")");
          Collection.Remove(item);
          break;
        }
      }
    }

    internal bool Contains(Guid id)
    {
      EvaDebug.DebugLog("Contains()");

      for (var i = 0; i < Collection.Count; i++)
      {
        if (Collection[i].FlightId == id)
        {
          return true;
        }
      }

      return false;
    }


    internal EvaContainer GetEva(Guid flightId)
    {
      for (var i = 0; i < Collection.Count; i++)
      {
        if (Collection[i].FlightId == flightId)
        {
          return Collection[i];
        }
      }

      return null;
    }

    /// <summary>
    ///   Load the list
    /// </summary>
    private void onFlightReadyCallback()
    {
      //Load the eva list.
      EvaDebug.DebugLog("onFlightReadyCallback()");
      EvaSettings.Load();
    }
  }
}