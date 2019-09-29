using System;

namespace MSD.EvaFollower.ControlTypes
{
  /// <summary>
  /// The object responsible for Formations.
  /// </summary>
  class EvaFormation : IEvaControlType
  {
    private EvaContainer _leader;

    /// <summary>
    /// Get the next position to walk to. 
    /// Formation should handle differents positions.
    /// </summary>
    /// <returns></returns>
    public Vector3d GetNextTarget()
    {
      if (_leader == null)
      {
        return Vector3d.zero;
      }

      //get the leader. 
      var target = _leader.EVA.vessel.GetWorldPos3D();

      //update move vector.
      return target;
    }

    public void SetLeader(EvaContainer leader)
    {
      this._leader = leader;
    }

    public string GetLeader()
    {
      if (_leader != null)
      {
        if (_leader.Loaded)
        {
          return _leader.EVA.name;
        }
      }

      return "None";
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
      string leaderId = "null";
      if (_leader != null)
      {
        leaderId = _leader.FlightId.ToString();
      }

      return "(Leader:" + leaderId + ")";
    }

    public void FromSave(string formation)
    {
      try
      {
        //EvaDebug.DebugWarning("Formation.FromSave()");
        formation = formation.Remove(0, 7); //Leader:

        if (formation != "null")
        {
          Guid flightId = new Guid(formation);
          EvaContainer container = EvaController.Instance.GetEva(flightId);

          if (container != null)
          {
            _leader = container;
          }
        }
      }
      catch
      {
        throw new Exception("[EFX] Formation.FromSave Failed.");
      }
    }
  }
}