using System;

namespace MSD.EvaFollower.ControlTypes
{
  /// <summary>
  ///   The mode the EVA is in.
  /// </summary>
  [Flags]
  internal enum Mode
  {
    None = 1,
    Follow = 2,
    Patrol = 3,
    Leader = 4,
    Order = 5,
    Wander = 6
  }

  /// <summary>
  ///   The status the EVA is in.
  /// </summary>
  [Flags]
  internal enum Status
  {
    None,
    Removed
  }

  /// <summary>
  ///   The animation states for the EvaControllerContainer
  /// </summary>
  internal enum AnimationState
  {
    None,
    Swim,
    Run,
    Walk,
    BoundSpeed,
    Idle
  }
}