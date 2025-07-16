using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MSD.EvaFollower
{
    static class KerbalEvaExtentions
    {
        public static Mesh helmetMesh = null;
        public static Mesh visorMesh = null;
        
        /// <summary>
        /// Enable you to remove the helmet.
        /// </summary>
        /// <param name="eva"></param>
        /// <param name="showHelmet"></param>
        public static void ShowHelmet(this KerbalEVA eva,bool showHelmet)
        {             
            foreach (Renderer renderer in eva.GetComponentsInChildren<Renderer>())
            {
                var smr = renderer as SkinnedMeshRenderer;

                if (smr != null)
                {
                    switch (smr.name)
                    {
                        case "helmet":
                            {
                                if (helmetMesh == null)
                                    helmetMesh = smr.sharedMesh;

                                smr.sharedMesh = showHelmet ? helmetMesh : null; 
                            }break;
                        case "visor":
                            {
                                if (visorMesh == null)
                                    visorMesh = smr.sharedMesh;

                                smr.sharedMesh = showHelmet ? visorMesh : null;
                            } break;
                    }
                }
            }

            if (!showHelmet)
            {
                eva.TurnLamp(false);
            }
        }

        /// <summary>
        /// Jump the current kerbal.
        /// </summary>
        /// <param name="eva"></param>
        public static void Jump(this KerbalEVA eva)
        {
            try
            {
                foreach (var item in eva.fsm.CurrentState.StateEvents)
                {
                    if (item.name == Localizer.Format("#LOC_EVAFollower_17"))
                    {
                        eva.fsm.RunEvent(item);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Let the current kerbal grap a nearby ladder.
        /// </summary>
        /// <param name="eva"></param>
        public static void GrapLadder(this KerbalEVA eva)
        {
            try
            {
                foreach (var item in eva.fsm.CurrentState.StateEvents)
                {
                    if (item.name == Localizer.Format("#LOC_EVAFollower_18"))
                    {
                        eva.fsm.RunEvent(item);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Let the current kerbal release the ladder, if on it.
        /// </summary>
        /// <param name="eva"></param>
        public static void ReleaseLadder(this KerbalEVA eva)
        {
            try
            {
                foreach (var item in eva.fsm.CurrentState.StateEvents)
                {
                    if (item.name == Localizer.Format("#LOC_EVAFollower_19"))
                    {
                        eva.fsm.RunEvent(item);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Toggle the jetpack. 
        /// </summary>
        /// <param name="eva"></param>
        public static void PackToggle(this KerbalEVA eva)
        {
            try
            {
                foreach (var item in eva.fsm.CurrentState.StateEvents)
                {
                    if (item.name == Localizer.Format("#LOC_EVAFollower_20"))
                    {
                        eva.fsm.RunEvent(item);
                    }
                }
            }
            catch { }
        }

        public static void RecoverFromRagdoll(this KerbalEVA eva)
        {
			Rigidbody rigidbody = null;
			eva.GetComponentCached<Rigidbody> (ref rigidbody);
			if (rigidbody != null) {
				if (rigidbody.isKinematic)
					return;
			}

            if (!eva.isEnabled)
                return;

            //Much Kudos to Razchek for finally slaying the Ragdoll Monster!
            if (eva.canRecover && eva.fsm.TimeAtCurrentState > 1.21f && !eva.part.GroundContact)
            {
                foreach (KFSMEvent stateEvent in eva.fsm.CurrentState.StateEvents)
                {
                    if (stateEvent.name == Localizer.Format("#LOC_EVAFollower_21"))
                    {
                        eva.fsm.RunEvent(stateEvent);
                        break;
                    }
                }

            }
        }

        public static void Animate(this KerbalEVA eva, AnimationState state)
        {
            string anim = Localizer.Format("#LOC_EVAFollower_22");

            switch (state)
            {
                case AnimationState.None: { } break;
                case AnimationState.Swim: { anim = "swim_forward"; } break;
                case AnimationState.Run: { anim = "wkC_run"; } break;
                case AnimationState.Walk: { anim = "wkC_forward"; } break;
                case AnimationState.BoundSpeed: { anim = "wkC_loG_forward"; } break;
                case AnimationState.Idle:
                    {
                        if (eva.part.WaterContact)
                            anim = "swim_idle";// NO_LOCALIZATION
                        else if (eva.JetpackDeployed)
                            anim = "jp_suspended";// NO_LOCALIZATION
                        else
                            anim = "idle";// NO_LOCALIZATION

                    } break;
            }

			Animation _animation = null;
			eva.GetComponentCached<Animation> (ref _animation);
			if (_animation != null) 
			{
				_animation.CrossFade (anim);
			}
		}

        /// <summary>
        /// Toggle the light of the current kerbal.
        /// </summary>
        /// <param name="eva"></param>
        public static void ToggleLight(this KerbalEVA eva)
        {
            eva.lampOn = !eva.lampOn;
            eva.TurnLamp(eva.lampOn);
        }

        /// <summary>
        /// Turn the lamp on of a kerbal.
        /// </summary>
        /// <param name="eva"></param>
        /// <param name="lampOn"></param>
        public static void TurnLamp(this KerbalEVA eva, bool lampOn)
        {
            eva.lampOn = lampOn;
            eva.headLamp.SetActive(lampOn);
        }

        /// <summary>
        /// Doesn't work ... yet!
        /// </summary>
        /// <param name="eva"></param>
        /// <param name="fear"></param>
        public static void FearFactor(this KerbalEVA eva, float fear)
        {
            var expS = UnityEngine.Object.FindObjectsOfType<kerbalExpressionSystem>();

            foreach (var item in expS)
            {
                if (item.kerbalEVA == eva)
                {
                    item.flight_gee = 10000;
                }
            }
        }



    }
}
