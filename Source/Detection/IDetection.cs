using System.Collections.Generic;

namespace MSD.EvaFollower.Detection
{
	interface IDetection
	{
		void UpdateMap (List<EvaContainer> collection);
		void Debug();
	}
}

