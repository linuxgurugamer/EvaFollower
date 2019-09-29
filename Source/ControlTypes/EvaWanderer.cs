using System;

namespace MSD.EvaFollower.ControlTypes
{
	public class EvaWanderer : IEvaControlType
	{     
		static readonly Random _random = new Random();

		internal Vector3d Position;
		internal KerbalEVA Eva;
		internal float Elapsed = 0;
		internal string ReferenceBody;

		bool _busy;

		public void SetEva (KerbalEVA eva)
		{
			this.Eva = eva;
			GenerateNewPosition ();
		}
			
		public bool CheckDistance (double sqrDistance)
		{
			if (sqrDistance < 3.0)
			{
				_busy = false;
				return true;
			}
			return false;
		}

		public Vector3d GetNextTarget ()
		{
			if (!_busy) {
				GenerateNewPosition ();
			}

			return Position;
		}

		public string ToSave()
		{
			return "(" + ")";
		}

		private void GenerateNewPosition(){
			Vector3d position = Eva.vessel.CoMD;

			//Vector3d eastUnit = eva.vessel.mainBody.getRFrmVel(position).normalized; //uses the rotation of the body's frame to determine "east"
			//Vector3d upUnit = (eva.vessel - eva.vessel.mainBody.position).normalized;
			//Vector3d northUnit = Vector3d.Cross(upUnit, eastUnit); //north = up cross east

			var offset = new Vector3d (
				(_random.NextDouble () * 2 - 1) * 100,	
				0,	
				(_random.NextDouble () * 2 - 1) * 100
			);

			var str = Environment.NewLine + Eva.vessel.transform.up.ToString ();
			str += Environment.NewLine + Eva.vessel.transform.forward.ToString ();
			str += Environment.NewLine + Eva.vessel.transform.right.ToString ();

			EvaController.Instance.Debug = str;

			Position = position;
			Position += offset;
		}

		private void SetReferenceBody()
		{
			if (this.ReferenceBody == "None")
			{
				this.ReferenceBody = FlightGlobals.ActiveVessel.mainBody.bodyName;
			}
		}

		public void FromSave(string action){

		}
	}
}

