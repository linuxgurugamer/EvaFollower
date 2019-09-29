using System;
using MSD.EvaFollower.ControlTypes;
using MSD.EvaFollower.Extentions;
using UnityEngine;

namespace MSD.EvaFollower
{
  [KSPAddon(KSPAddon.Startup.Flight, false)]
  public class EvaLogic : MonoBehaviour
  {
    //List<IDetection> detectionSystems = new List<IDetection>();

    public void Start()
    {
      EvaDebug.DebugWarning("EvaLogic.Start()");
    }

    public void OnDestroy()
    {
      EvaDebug.DebugWarning("EvaLogic.OnDestroy()");
    }

    public void FixedUpdate()
    {
      // Update detection systems.
      //foreach (var detection in detectionSystems) {
      //	detection.UpdateMap (EvaController.instance.collection);
      //}
    }

    public void Update()
    {
      if (!FlightGlobals.ready || PauseMenu.isOpen)
      {
        return;
      }

      // Replace this with a check to see if GUI is hidden
      if (Input.GetKeyDown(KeyCode.F2) && HighLogic.CurrentGame.Parameters.CustomParams<EvaFollowerMiscSettings>()
            .DisplayDebugLinesSetting)
      {
        EvaSettings.DisplayDebugLines = !EvaSettings.DisplayDebugLines;
        foreach (var container in EvaController.Instance.Collection)
        {
          container.togglePatrolLines();
        }
      }

      if (Input.GetKeyDown(KeyCode.B))
      {
        foreach (var container in EvaController.Instance.Collection)
        {
          container.EVA.PackToggle();
        }
      }

      try
      {
        foreach (var eva in EvaController.Instance.Collection.ToArray())
        {
          if (eva == null)
          {
            //is this possible ?
            EvaDebug.DebugWarning("eva == null");
            continue;
          }

          //skip unloaded vessels
          if (!eva.Loaded)
          {
            continue;
          }

          //Turn the lights on when dark.
          //Skip for now, too buggy..
          //eva.UpdateLamps();

          if (eva.Mode == Mode.None)
          {
            //Nothing to do here.
            continue;
          }

          //Recover from ragdoll, if possible.
          if (eva.IsRagDoll)
          {
            eva.RecoverFromRagdoll();
            continue;
          }

          var move = -eva.Position;

          //Get next Action, Formation or Patrol
          var target = eva.GetNextTarget();

          // Path Finding
          //todo: check if the target is occopied.
          move += target;

          var sqrDist = move.sqrMagnitude;
          var speed = TimeWarp.deltaTime;

          if (eva.OnALadder)
          {
            eva.ReleaseLadder();
          }

          #region Break Free Code

          if (eva.IsActive)
          {
            var mode = eva.Mode;

            if (Input.GetKeyDown(KeyCode.W))
            {
              mode = Mode.None;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
              mode = Mode.None;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
              mode = Mode.None;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
              mode = Mode.None;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
              mode = Mode.None;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
              mode = Mode.None;
            }

            if (mode == Mode.None)
            {
              //break free!
              eva.Mode = mode;
              continue;
            }
          }

          #endregion

          //Animation Logic
          eva.UpdateAnimations(sqrDist, ref speed);

          move.Normalize();

          //Distance Logic
          eva.CheckDistance(move, speed, sqrDist);

          //Reset Animation Mode Events
          eva.CheckModeIsNone();
        }
      }
      catch (Exception exp)
      {
        EvaDebug.DebugWarning("[EFX] EvaLogic: " + exp.Message + ":" + exp);
      }
    }
  }
}