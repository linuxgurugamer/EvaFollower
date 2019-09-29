using System;
using MSD.EvaFollower.ControlTypes;
using MSD.EvaFollower.Extentions;
using UnityEngine;
using AnimationState = MSD.EvaFollower.ControlTypes.AnimationState;

namespace MSD.EvaFollower
{
  internal class EvaContainer
  {
    private const float RunMultiplier = 1.75f;
    private const float BoundSpeedMultiplier = 1.25f;
    public Guid FlightId;
    public Mode Mode = Mode.None;
    public Status Status = Status.None;

    internal EvaFormation Formation = new EvaFormation();
    internal EvaPatrol Patrol = new EvaPatrol();
    internal EvaOrder Order = new EvaOrder();
    internal EvaWanderer Wanderer = new EvaWanderer();

    private bool _loaded;

    public EvaContainer(Guid flightId)
    {
      FlightId = flightId;
      _loaded = false;
    }

    public bool IsActive
    {
      get { return EVA.vessel == FlightGlobals.ActiveVessel; }
    }

    public bool IsRagDoll
    {
      get { return EVA.isRagdoll; }
    }

    public bool AllowPatrol
    {
      get { return Patrol.Actions.Count >= 1; }
    }

    public bool AllowRunning
    {
      get
      {
        if (Mode == Mode.Patrol)
        {
          return Patrol.AllowRunning;
        }

        if (Mode == Mode.Order)
        {
          return Order.AllowRunning;
        }

        return false;
      }
    }

    public KerbalEVA EVA { get; private set; }

    public bool Selected { get; set; }

    /// <summary>
    ///   Get the world position of the kerbal.
    /// </summary>
    public Vector3d Position
    {
      get { return EVA.vessel.GetWorldPos3D(); }
    }

    public bool Loaded
    {
      get
      {
        var isLoaded = _loaded;

        if (_loaded)
        {
          isLoaded |= EVA.isEnabled;
        }

        return isLoaded;
      }
    }

    public string Name { get; set; }

    public bool OnALadder
    {
      get { return EVA.OnALadder; }
    }

    public void togglePatrolLines()
    {
      if (EvaSettings.DisplayDebugLines)
      {
        Patrol.GenerateLine();
      }
      else
      {
        Patrol.Hide();
      }
    }

    public void Load(KerbalEVA eva)
    {
      //Load KerbalEVA.
      EVA = eva;
      _loaded = true;

      //Set Name
      Name = eva.name;

      //module on last.
      var module = (EvaModule) eva.GetComponent(typeof(EvaModule));
      module.Load(this);

      EvaDebug.DebugWarning("EvaContainer.Load(" + eva.name + ")");
    }

    public void Unload()
    {
      EvaDebug.DebugWarning("EvaContainer.Unload(" + EVA.name + ")");
      _loaded = false;
    }

    internal string ToSave()
    {
      return FlightId + "," + Name + "," + Mode + "," + Status + "," + Selected + ", True,"
             + Formation.ToSave() + ","
             + Patrol.ToSave() + ","
             + Order.ToSave() + ","
             + Wanderer.ToSave();
    }

    internal void FromSave(string evaSettings)
    {
      var reader = new EvaTokenReader(evaSettings);

      try
      {
        var sflightID = reader.NextTokenEnd(',');
        var sName = reader.NextTokenEnd(',');
        var mode = reader.NextTokenEnd(',');
        var status = reader.NextTokenEnd(',');
        var selected = reader.NextTokenEnd(',');

        // backwards compatible.
        var showHelmet = reader.NextTokenEnd(',');

        var formation = reader.NextToken('(', ')');
        reader.Consume();
        var patrol = reader.NextToken('(', ')');
        reader.Consume();
        var order = reader.NextToken('(', ')');
        reader.Consume();
        var wanderer = reader.NextToken('(', ')');

        Name = sName;
        Mode = (Mode) Enum.Parse(typeof(Mode), mode);
        Status = (Status) Enum.Parse(typeof(Status), status);
        Selected = bool.Parse(selected);


        Formation.FromSave(formation);
        Patrol.FromSave(patrol);
        Order.FromSave(order);
        Wanderer.FromSave(wanderer);

        EvaDebug.DebugLog("Loaded: " + mode);
        EvaDebug.DebugLog("name: " + sName);
        EvaDebug.DebugLog("status: " + status);
        EvaDebug.DebugLog("selected: " + selected);
      }
      catch
      {
        throw new Exception("[EFX] FromSave Failed.");
      }
    }


    internal void Follow()
    {
      var flightID = FlightGlobals.fetch.activeVessel.id;
      var leader = EvaController.Instance.GetEva(flightID);

      Selected = false;
      Mode = Mode.Follow;
      Formation.SetLeader(leader);
    }

    internal void Stay()
    {
      Mode = Mode.None;
    }

    internal void SetWaypoint()
    {
      Patrol.Move(EVA.vessel);
    }

    internal void Wait()
    {
      Patrol.Wait(EVA.vessel);
    }

    internal void StartPatrol()
    {
      Mode = Mode.Patrol;
    }

    internal void EndPatrol()
    {
      Mode = Mode.None;
      Patrol.Clear();
      EVA.Animate(AnimationState.Idle);
    }

    internal void SetRunPatrolMode()
    {
      Patrol.AllowRunning = true;
    }

    internal void SetWalkPatrolMode()
    {
      Patrol.AllowRunning = false;
    }

    internal void StartWanderer()
    {
      Mode = Mode.Wander;
      Wanderer.SetEva(EVA);
    }

    internal void UpdateLamps()
    {
      var lampOn = Util.IsDark(EVA.transform);
      EVA.TurnLamp(lampOn);
    }

    internal void RecoverFromRagdoll()
    {
      EVA.RecoverFromRagdoll();
    }

    internal Vector3d GetNextTarget()
    {
      switch (Mode)
      {
        case Mode.Follow:
        {
          var target = Formation.GetNextTarget();

          if (target == Vector2d.zero)
          {
            Mode = Mode.None;
          }

          return target;
        }
        case Mode.Patrol:
          return Patrol.GetNextTarget();
        case Mode.Order:
          return Order.GetNextTarget();
        case Mode.Wander:
          return Wanderer.GetNextTarget();
        default:
          //Error
          throw new Exception("[EFX] New Mode Introduced");
      }
    }

    internal void ReleaseLadder()
    {
      EVA.ReleaseLadder();
    }

    internal void UpdateAnimations(double sqrDist, ref float speed)
    {
      var geeForce = FlightGlobals.currentMainBody.GeeASL;

      if (EVA.part.WaterContact)
      {
        speed *= EVA.swimSpeed;
        EVA.Animate(AnimationState.Swim);
      }
      else if (EVA.JetpackDeployed)
      {
        speed *= 1f;
        EVA.Animate(AnimationState.Idle);
      }
      else if (geeForce >= EVA.minRunningGee) //sqrDist > 5f &&
      {
        if (AllowRunning)
        {
          speed *= EVA.runSpeed;
          EVA.Animate(AnimationState.Run);
        }
        else if (sqrDist > 4 && Mode == Mode.Follow)
        {
          speed *= EVA.runSpeed * RunMultiplier;
          EVA.Animate(AnimationState.Run);
        }
        else if (sqrDist > 8f && Mode == Mode.Follow)
        {
          speed *= EVA.runSpeed * RunMultiplier;
          EVA.Animate(AnimationState.Run);
        }
        else
        {
          speed *= EVA.walkSpeed;
          EVA.Animate(AnimationState.Walk);
        }
      }
      else if (geeForce >= EVA.minWalkingGee)
      {
        speed *= EVA.walkSpeed;
        EVA.Animate(AnimationState.Walk);
      }
      else
      {
        speed *= EVA.boundSpeed * BoundSpeedMultiplier; 
        EVA.Animate(AnimationState.BoundSpeed);
      }
    }

    internal void CheckDistance(Vector3d move, float speed, double sqrDist)
    {
      IEvaControlType controlType;

      switch (Mode)
      {
        case Mode.Follow:
          controlType = Formation;
          break;
        case Mode.Patrol:
          controlType = Patrol;
          break;
        case Mode.Order:
          controlType = Order;
          break;
        case Mode.Wander:
          controlType = Wanderer;
          break;
        default:
          throw new Exception("[EFX] New Mode Introduced");
      }

      if (controlType.CheckDistance(sqrDist))
      {
        EVA.Animate(AnimationState.Idle);

        if (controlType is EvaOrder)
        {
          Mode = Mode.None;
        }
      }
      else
      {
        if (AbleToMove())
        {
          Move(move, speed);
        }
      }
    }

    /// <summary>
    ///   Move the current kerbal to target.
    /// </summary>
    /// <param name="move"></param>
    /// <param name="speed"></param>
    internal void Move(Vector3d move, float speed)
    {
      #region Move & Rotate Kerbal

      //speed values
      move *= speed;

      //rotate
      if (move != Vector3d.zero)
      {
        if (EVA.JetpackDeployed)
        {
          EVA.PackToggle();
        }
        else
        {
          //rotation
          var from = EVA.part.vessel.transform.rotation;
          var to = Quaternion.LookRotation(move, EVA.fUp);
          var result = Quaternion.RotateTowards(from, to, EVA.turnRate);

          EVA.part.vessel.SetRotation(result);

          Rigidbody rigidbody = null;
          EVA.GetComponentCached(ref rigidbody);

          //move
          if (rigidbody != null)
          {
            rigidbody.MovePosition(rigidbody.position + move);
          }
        }
      }

      #endregion
    }

    internal void CheckModeIsNone()
    {
      if (Mode == Mode.None)
      {
        EVA.Animate(AnimationState.Idle);
      }
    }

    internal void SetOrder(Vector3d position, Vector3d vector3D)
    {
      Order.Move(position, vector3D);
    }

    private bool AbleToMove()
    {
      Rigidbody rigidbody = null;
      EVA.GetComponentCached(ref rigidbody);
      return !EVA.isEnabled | !EVA.isRagdoll | !rigidbody.isKinematic;
    }
  }
}