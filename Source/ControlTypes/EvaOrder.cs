using KSP.Localization;
using System.Xml;
using System;

namespace MSD.EvaFollower
{
    /// <summary>
    /// The object responsible for ordering the kerbal around. 
    /// </summary>
    class EvaOrder : IEvaControlType
    {
        internal Vector3d Offset = -Vector3d.one;
        internal Vector3d Position;

        public bool AllowRunning { get; set; }

        public EvaOrder()
        {
            AllowRunning = true;
        }

        public bool CheckDistance(double sqrDistance)
        {
            bool complete = (sqrDistance < 0.8);            
            return complete;
        }

        public Vector3d GetNextTarget()
        {
            return Position;
        }

        public void Move(Vector3d pos, Vector3d off)
        {
            this.Offset = off;
            this.Position = pos;
        }

        public override string ToString()
        {
            return Position + Localizer.Format("#LOC_EVAFollower_13") + Offset + ")";
        }

		public string ToSave()
        {
            return "(" + AllowRunning.ToString() + "," + Position + "," + Offset + ")";
        }

		public void FromSave(string order)
        {
            //EvaDebug.DebugWarning("Order.FromSave()");
            EvaTokenReader reader = new EvaTokenReader(order);

            string sAllowRunning = reader.NextTokenEnd(',');
            string sPosition = reader.NextToken('[', ']'); reader.Consume(); // , 
            string sOffset = reader.NextToken('[', ']');

            AllowRunning = bool.Parse(sAllowRunning);
            Position = Util.ParseVector3d(sPosition, false);
            Offset = Util.ParseVector3d(sOffset, false);                        
        }
    }
}
