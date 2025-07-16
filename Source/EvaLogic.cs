using UnityEngine;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace MSD.EvaFollower
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class EvaLogic : MonoBehaviour
    {
        //List<IDetection> detectionSystems = new List<IDetection>();

        public EvaLogic()
        {

            //detectionSystems.Add (new DeadSpaceDetection ());
        }

        public void Start()
        {
            EvaDebug.DebugWarning("EvaLogic.Start()");
            GameEvents.onHideUI.Add(onHideUI);
        }
        bool uiHidden = false;
        void onHideUI()
        {
            uiHidden = !uiHidden;
        }
        public void OnDestroy()
        {
            EvaDebug.DebugWarning("EvaLogic.OnDestroy()");
            GameEvents.onHideUI.Remove(onHideUI);
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
                return;

            // Replace this with a check to see if GUI is hidden
            if (EvaSettings.displayDebugLines == uiHidden && HighLogic.CurrentGame.Parameters.CustomParams<EvaFollowerMiscSettings>().displayDebugLinesSetting)
            {

                //EvaSettings.displayDebugLines = !EvaSettings.displayDebugLines;
                EvaSettings.displayDebugLines = !uiHidden;
                foreach (EvaContainer container in EvaController.instance.collection)
                {
                    container.togglePatrolLines();
                }
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                foreach (EvaContainer container in EvaController.instance.collection)
                {
                    container.EVA.PackToggle();
                }
            }

            try
            {
                foreach (EvaContainer eva in EvaController.instance.collection.ToArray())
                {
                    if (eva == null)
                    {
                        //is this possible ?
                        EvaDebug.DebugWarning("eva == null"); // NO_LOCALIZATION
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

                    if (eva.mode == Mode.None)
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

                    Vector3d move = -eva.Position;

                    //Get next Action, Formation or Patrol
                    Vector3d target = eva.GetNextTarget();

                    // Path Finding
                    //todo: check if the target is occopied.
                    move += target;

                    double sqrDist = move.sqrMagnitude;
                    float speed = TimeWarp.deltaTime;

                    if (eva.OnALadder)
                    {
                        eva.ReleaseLadder();
                    }

                    #region Break Free Code

                    if (eva.IsActive)
                    {
                        Mode mode = eva.mode;

                        if (Input.GetKeyDown(KeyCode.W))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyDown(KeyCode.S))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyDown(KeyCode.A))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyDown(KeyCode.D))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyDown(KeyCode.Q))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyDown(KeyCode.E))
                            mode = EvaFollower.Mode.None;

                        if (mode == Mode.None)
                        {
                            //break free!
                            eva.mode = mode;
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
                EvaDebug.DebugWarning("[EFX] EvaLogic: " + exp.Message + ":" + exp.ToString());// NO_LOCALIZATION
            }
        }
    }
}
