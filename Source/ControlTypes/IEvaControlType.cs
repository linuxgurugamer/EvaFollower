namespace MSD.EvaFollower.ControlTypes
{
  interface IEvaControlType
  {
    /// <summary>
    /// Check if the criteria is met, 
    /// </summary>
    /// <param name="sqrDistance">The distance between position and target</param>
    /// <returns>Returns true if the criteria is met.</returns>
    bool CheckDistance(double sqrDistance);

    /// <summary>
    /// Get the next target to move on. 
    /// </summary>
    Vector3d GetNextTarget();

    string ToSave();
    void FromSave(string action);
  }
}
