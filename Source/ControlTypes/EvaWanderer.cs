using KSP.Localization;
using System;

namespace MSD.EvaFollower
{
	public class EvaWanderer : IEvaControlType
	{     
		static Random random = new Random();

		internal Vector3d Position;
		internal KerbalEVA eva;
		internal float elapsed = 0;
		internal string referenceBody;

		bool busy = false;

		public EvaWanderer ()
		{
		}

		public void SetEva (KerbalEVA eva)
		{
			this.eva = eva;
			GenerateNewPosition ();
		}
			
		public bool CheckDistance (double sqrDistance)
		{
			if (sqrDistance < 3.0)
			{
				busy = false;
				return true;
			}
			return false;
		}

		public Vector3d GetNextTarget ()
		{
			if (!busy) {
				GenerateNewPosition ();
			}

			return Position;
		}

		public string ToSave()
		{
			return "(" + ")";
		}

		private void GenerateNewPosition(){
			Vector3d position = eva.vessel.CoMD;

			//Vector3d eastUnit = eva.vessel.mainBody.getRFrmVel(position).normalized; //uses the rotation of the body's frame to determine "east"
			//Vector3d upUnit = (eva.vessel - eva.vessel.mainBody.position).normalized;
			//Vector3d northUnit = Vector3d.Cross(upUnit, eastUnit); //north = up cross east

			var offset = new Vector3d (
				(((random.NextDouble () * 2 ) - 1) * 100),	
				0,	
				(((random.NextDouble () * 2 ) - 1) * 100)
			);

			var str = Environment.NewLine + eva.vessel.transform.up.ToString ();
			str += Environment.NewLine + eva.vessel.transform.forward.ToString ();
			str += Environment.NewLine + eva.vessel.transform.right.ToString ();

			//EvaController.instance.debug = str;

			Position = position;
			Position += offset;
		}

		private void SetReferenceBody()
		{
			if (this.referenceBody == Localizer.Format("#LOC_EVAFollower_1"))
			{
				this.referenceBody = FlightGlobals.ActiveVessel.mainBody.bodyName;
			}
		}

		public void FromSave(string action){

		}
	}
}

