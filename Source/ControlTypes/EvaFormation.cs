using KSP.Localization;
using System;
using System.Xml;

namespace MSD.EvaFollower
{
    /// <summary>
    /// The object responsible for Formations.
    /// </summary>
    class EvaFormation : IEvaControlType
    {
        private EvaContainer leader;

        /// <summary>
        /// Get the next position to walk to. 
        /// Formation should handle differents positions.
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public Vector3d GetNextTarget()
        {           
            if (leader == null)
            {
                return Vector3d.zero;
            }
            
            //get the leader. 
            var target = leader.EVA.vessel.GetWorldPos3D();        
            
            //update move vector.
            return target;
        }

        public void SetLeader(EvaContainer leader)
        {
            this.leader = leader;
        }

        public string GetLeader()
        {
            if (leader != null)
            {
                if (leader.Loaded)
                {
                    return leader.EVA.name;
                }
            }

            return Localizer.Format("#LOC_EVAFollower_1");
        }

        /// <summary>
        /// Check if the distance to the target is reached.
        /// </summary>
        /// <param name="sqrDistance"></param>
        /// <returns></returns>
        public bool CheckDistance(double sqrDistance)
        {
            if (sqrDistance < 3.0)
            {
                return true;
            }
            return false;
        }

        
        public string ToSave()
        {
            string leaderID = Localizer.Format("#LOC_EVAFollower_11");
            if(leader != null)
            {
                leaderID = leader.flightID.ToString();
            }
            return Localizer.Format("#LOC_EVAFollower_12") + leaderID + ")";
        }

		public void FromSave(string formation)
        {
            try
            {
                //EvaDebug.DebugWarning("Formation.FromSave()");
                formation = formation.Remove(0, 7); //Leader:
                
                if (formation != Localizer.Format("#LOC_EVAFollower_11"))
                {
                    Guid flightID = new Guid(formation);
                    EvaContainer container = EvaController.instance.GetEva(flightID);

                    if (container != null)
                    {
                        leader = container;
                    }
                }
            }
            catch
            {
                throw new Exception("[EFX] Formation.FromSave Failed."); // NO_LOCALIZATION
            }  
        }
    }
}
