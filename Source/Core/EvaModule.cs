using MSD.EvaFollower.ControlTypes;

namespace MSD.EvaFollower
{
  /// <summary>
  /// Keep track of the Context Menu.
  /// </summary>
  class EvaModule : PartModule
  {
    private EvaContainer _currentContainer;

    public void Update()
    {
      if (!FlightGlobals.ready || PauseMenu.isOpen)
        return;

      if (_currentContainer == null)
        return;

      ResetEvents();
      SetEvents();

    }

    public void Load(EvaContainer current)
    {
      this._currentContainer = current;
    }

    /// <summary>
    /// The default events based on the kerbal status.
    /// </summary>
    public void ResetEvents()
    {
      Events["Follow"].active = false;
      Events["Stay"].active = false;
      Events["SetPoint"].active = false;
      Events["Wait"].active = false;
      Events["Patrol"].active = false;
      Events["EndPatrol"].active = false;
      Events["PatrolRun"].active = false;
      Events["PatrolWalk"].active = false;
      Events["StartWanderer"].active = true;
    }

    /// <summary>
    /// Set events based on the kerbal status.
    /// </summary>
    public void SetEvents()
    {
      if (!_currentContainer.Loaded)
        return;

      if (!_currentContainer.EVA.vessel.Landed)
      {
        return;
      }

      if (_currentContainer.Mode == Mode.None)
      {
        Events["Follow"].active = true;
        Events["Stay"].active = false;
        //Events["StartWanderer"].active = true;
      }
      else if (_currentContainer.Mode == Mode.Follow)
      {
        Events["Follow"].active = false;
        Events["Stay"].active = true;
      }
      else if (_currentContainer.Mode == Mode.Patrol)
      {
        if (_currentContainer.AllowRunning)
        {
          Events["PatrolWalk"].active = true;
        }
        else
        {
          Events["PatrolRun"].active = true;
        }

        Events["Patrol"].active = false;
        Events["EndPatrol"].active = true;
      }
      else if (_currentContainer.Mode == Mode.Order)
      {
        Events["Stay"].active = true;
        Events["Follow"].active = true;
      }

      if (_currentContainer.IsActive)
      {
        Events["Follow"].active = false;
        Events["Stay"].active = false;
        Events["SetPoint"].active = true;
        Events["Wait"].active = true;

        if (_currentContainer.Mode != Mode.Patrol)
        {
          if (_currentContainer.AllowPatrol)
          {
            Events["Patrol"].active = true;
          }
        }
        else
        {
          Events["SetPoint"].active = false;
          Events["Wait"].active = false;
        }
      }
    }


    [KSPEvent(guiActive = true, guiName = "Follow Me", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
    public void Follow()
    {
      _currentContainer.Follow();
    }

    [KSPEvent(guiActive = true, guiName = "Stay Put", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
    public void Stay()
    {
      _currentContainer.Stay();
    }

    [KSPEvent(guiActive = true, guiName = "Add Waypoint", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
    public void SetPoint()
    {
      _currentContainer.SetWaypoint();
    }

    [KSPEvent(guiActive = true, guiName = "Wait", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
    public void Wait()
    {
      _currentContainer.Wait();
    }

    [KSPEvent(guiActive = true, guiName = "Patrol", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
    public void Patrol()
    {
      _currentContainer.StartPatrol();
    }

    [KSPEvent(guiActive = true, guiName = "End Patrol", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
    public void EndPatrol()
    {
      _currentContainer.EndPatrol();
    }

    [KSPEvent(guiActive = true, guiName = "Walk", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
    public void PatrolWalk()
    {
      _currentContainer.SetWalkPatrolMode();
    }

    [KSPEvent(guiActive = true, guiName = "Run", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
    public void PatrolRun()
    {
      _currentContainer.SetRunPatrolMode();
    }

    [KSPEvent(guiActive = true, guiName = "Wander", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
    public void StartWanderer()
    {
      _currentContainer.StartWanderer();
    }

    
    [KSPEvent(guiActive = true, guiName = "Debug", active = true, guiActiveUnfocused = true, unfocusedRange = 8)]
    public void Debug()
    {
      
    }
  }
}