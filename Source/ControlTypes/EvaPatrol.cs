using System;
using System.Collections.Generic;
using MSD.EvaFollower.Extentions;
using UnityEngine;

namespace MSD.EvaFollower.ControlTypes
{
  /// <summary>
  /// The object responsible for Patroling the kerbal.
  /// </summary>
  class EvaPatrol : IEvaControlType
  {
    public bool AllowRunning { get; set; }
    public List<PatrolAction> Actions = new List<PatrolAction>();
    public int CurrentPatrolPoint;
    public string ReferenceBody = "None";

    private float _delta;

    public bool CheckDistance(double sqrDistance)
    {
      bool complete = (sqrDistance < 0.3);

      if (complete)
      {
        PatrolAction currentPoint = Actions[CurrentPatrolPoint];

        if (currentPoint.Type == PatrolActionType.Wait)
        {
          _delta += Time.deltaTime;

          if (_delta > currentPoint.Delay)
          {
            SetNextPoint();
            _delta = 0;
          }
        }
        else //move
        {
          SetNextPoint();
        }
      }

      return complete;
    }

    private void SetNextPoint()
    {
      ++CurrentPatrolPoint;

      if (CurrentPatrolPoint >= Actions.Count)
        CurrentPatrolPoint = 0;
    }

    public Vector3d GetNextTarget()
    {
      PatrolAction currentPoint = Actions[CurrentPatrolPoint];
      return Util.GetWorldPos3DLoad(currentPoint.Position);
    }

    public void Move(Vessel vessel)
    {
      SetReferenceBody();

      Vector3d position = Util.GetWorldPos3DSave(vessel);
      Actions.Add(new PatrolAction(PatrolActionType.Move, 0, position));

      if (EvaSettings.DisplayDebugLines)
        setLine(position);

    }

    public void Wait(Vessel vessel)
    {
      SetReferenceBody();

      Vector3d position = Util.GetWorldPos3DSave(vessel);
      Actions.Add(new PatrolAction(PatrolActionType.Wait, 1, position));

      if (EvaSettings.DisplayDebugLines)
        setLine(position);

    }

    private void SetReferenceBody()
    {
      if (this.ReferenceBody == "None")
      {
        this.ReferenceBody = FlightGlobals.ActiveVessel.mainBody.bodyName;
      }
    }

    public void Clear()
    {
      ReferenceBody = "None";

      CurrentPatrolPoint = 0;
      Actions.Clear();

      if (EvaSettings.DisplayDebugLines)
        _lineRenderer.positionCount = 0;

    }

    public void Hide()
    {
      _lineRenderer.positionCount = 0;
    }


    public string ToSave()
    {
      string actionList = "{";
      for (int i = 0; i < Actions.Count; i++)
      {
        actionList += Actions[i].ToSave();
      }

      actionList += "}";

      object[] args = {
        AllowRunning.ToString(),
        CurrentPatrolPoint.ToString(),
        ReferenceBody,
        actionList
      };

      return string.Format("({0}, {1}, {2}, {3})", args);
    }

    public void FromSave(string patrol)
    {
      try
      {
        //EvaDebug.DebugWarning("Patrol.FromSave()");
        EvaTokenReader reader = new EvaTokenReader(patrol);

        string sAllowRunning = reader.NextTokenEnd(',');
        string sCurrentPatrolPoint = reader.NextTokenEnd(',');
        string sReferenceBody = reader.NextTokenEnd(',');
        string sPointlist = reader.NextToken('{', '}');

        AllowRunning = bool.Parse(sAllowRunning);
        CurrentPatrolPoint = int.Parse(sCurrentPatrolPoint);
        ReferenceBody = sReferenceBody;

        Actions.Clear();

        if (!string.IsNullOrEmpty(sPointlist))
        {
          reader = new EvaTokenReader(sPointlist);

          while (!reader.EOF)
          {
            PatrolAction action = new PatrolAction();

            string token = reader.NextToken('(', ')');
            action.FromSave(token);

            Actions.Add(action);
          }


          if (EvaSettings.DisplayDebugLines)
            GenerateLine();
        }
      }
      catch
      {
        throw new Exception("[EFX] Patrol.FromSave Failed.");
      }
    }


    public void GenerateLine()
    {
      _lineRenderer.positionCount = Actions.Count + 1;

      for (int i = 0; i < Actions.Count; i++)
      {
        _lineRenderer.SetPosition(i, Util.GetWorldPos3DLoad(Actions[i].Position));
      }

      _lineRenderer.SetPosition(Actions.Count, Util.GetWorldPos3DLoad(Actions[0].Position));
    }


    LineRenderer _lineRenderer;

    private void setLine(Vector3d position)
    {
      _lineRenderer.positionCount = Actions.Count;
      _lineRenderer.SetPosition(Actions.Count - 1, Util.GetWorldPos3DLoad(position));
    }

    public EvaPatrol()
    {
      if (EvaSettings.DisplayDebugLines)
      {
        _lineRenderer = new GameObject().AddComponent<LineRenderer>();

        _lineRenderer.useWorldSpace = false;
        _lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;
        //lineRenderer.SetColors(Color.green, Color.red);
        _lineRenderer.startColor = Color.green;
        _lineRenderer.endColor = Color.red;


        Renderer renderer = null;
        _lineRenderer.GetComponentCached(ref renderer);

        if (renderer != null)
        {
          renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
          renderer.receiveShadows = false;
          renderer.enabled = true;
        }

        _lineRenderer.positionCount = 0;
      }
    }
  }

  internal class PatrolAction
  {
    public Vector3d Position;
    public PatrolActionType Type;
    public int Delay;

    public PatrolAction()
    {
      this.Type = PatrolActionType.Move;
      this.Delay = 10;
      this.Position = new Vector3d();
    }

    public PatrolAction(PatrolActionType type, int delay, Vector3d position)
    {
      this.Type = type;
      this.Delay = delay;
      this.Position = position;
    }

    internal string ToSave()
    {
      return "(" + Type.ToString() + "," + Delay.ToString() + "," + Position.ToString() + ")";
    }

    internal void FromSave(string action)
    {
      EvaTokenReader reader = new EvaTokenReader(action);

      string sType = reader.NextTokenEnd(',');
      string sDelay = reader.NextTokenEnd(',');
      string sPosition = reader.NextToken('[', ']');

      Type = (PatrolActionType) Enum.Parse(typeof(PatrolActionType), sType);
      Delay = int.Parse(sDelay);
      Position = Util.ParseVector3d(sPosition, false);
    }

    public override string ToString()
    {
      return "position = " + Position.ToString() + ", delay = " + Delay + ", type = " + Type.ToString();
    }
  }

  [Flags]
  internal enum PatrolActionType
  {
    Move,
    Wait,
  }
}