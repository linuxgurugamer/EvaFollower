using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Diagnostics;

namespace MSD.EvaFollower
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class EvaOrderController : MonoBehaviour
    {
        private Dictionary<Guid, LineRenderer> selectionLines = new Dictionary<Guid, LineRenderer>();
        private LineRenderer _cursor = new LineRenderer();

        //Selection variables
        private bool gameUIToggle = true;
        private Texture2D _selectionHighlight = new Texture2D(200, 200);
        private Rect _selection = new Rect(0, 0, 0, 0);
        private Vector3 _startClick = -Vector3.one;

        //Cursor variables
        private bool showCursor = false;
	
        private RaycastHit _cursorHit = new RaycastHit();
        private Vector3 _cursorPosition;
        private Quaternion _cursorRotation;
        private bool _animatedCursor = false;
        private float _animatedCursorValue = 0;
        private int selectedKerbals = 0;
        
        //animation variable
        private double angle = 0;

        
        public void Start()
        {
            EvaDebug.DebugWarning("EvaOrderController.Start()");// NO_LOCALIZATION

            //save config.
            //EvaSettings.SaveConfiguration();
            EvaSettings.LoadConfiguration();

            if (EvaSettings.displayDebugLines)
            {
                InitializeDebugLine();
            }

            InitializeCursor();
        }
        public void OnDestroy()
        {
            EvaDebug.DebugWarning("EvaOrderController.OnDestroy()");// NO_LOCALIZATION
        }

        private void InitializeCursor()
        {
            _cursor = new GameObject().AddComponent<LineRenderer>();

            _cursor.useWorldSpace = false;
            _cursor.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive")); // NO_LOCALIZATION
            //_cursor.SetWidth(0.05f, 0.05f);
            _cursor.startWidth = 0.05f;
            _cursor.endWidth = 0.05f;

            //_cursor.SetColors(Color.green, Color.green);
            _cursor.startColor = _cursor.endColor = Color.green;

			Renderer _renderer = null;
			_cursor.GetComponentCached<Renderer> (ref _renderer);

			if (_renderer != null) {
				_renderer.enabled = false;
				_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;				
				_renderer.receiveShadows = false;
			}
            int segments = 32;
            _cursor.positionCount = segments;

            CreateCircle(_cursor, segments, 0.125);
        }

        /// <summary>
        /// Set the properties of the target cursor.
        /// </summary>
        private void SetCursorProperties()
        {
            double v = 1.5 + Math.Sin(angle) * 0.25 + Math.Sin(_animatedCursorValue) * 2;

            _cursor.transform.localScale = new Vector3((float)v, (float)v, (float)v);
            _cursor.transform.rotation = _cursorRotation;
            _cursor.transform.position = _cursorPosition;

            SetCursorAnimation();
        }

        /// <summary>
        /// Display a little animation when target is clicked. 
        /// </summary>
        private void SetCursorAnimation()
        {
            //little hackish... 
            if (_animatedCursor)
            {
                _animatedCursorValue += (float)(Math.PI / 8.0f);

                Color c = new Color(_animatedCursorValue / 8.0f, 1.0f - _animatedCursorValue / 8.0f, 0);
                //_cursor.SetColors(c, c);
                _cursor.startColor = _cursor.endColor = c;

                //show the animation and close the cursor 
                if (_animatedCursorValue > 20)
                {
                    DisableCursor();
                }
            }
        }

        /// <summary>
        /// Update the selection model position.
        /// </summary>
        /// <param name="eva"></param>
        public void UpdateSelectionLine(EvaContainer container)
        {
            if (!selectionLines.ContainsKey(container.flightID))
            {
                CreateLine(container);
            }

            var lineRenderer = selectionLines[container.flightID];
            SetSelectionLineProperties(container.EVA, lineRenderer);            
        }

        /// <summary>
        /// Set the position of the selector. 
        /// </summary>
        /// <param name="eva"></param>
        /// <param name="lineRenderer"></param>
        private void SetSelectionLineProperties(KerbalEVA eva, LineRenderer lineRenderer)
        {
            double v = 1.5 + Math.Sin(angle) * 0.25;
            var p = eva.transform.position;
            var r = eva.transform.rotation;
            var f = eva.transform.forward;

            lineRenderer.transform.localScale = new Vector3((float)v, (float)v, (float)v);
            lineRenderer.transform.rotation = r * Quaternion.Euler(0,(float)(angle*60.0),0);;
            lineRenderer.transform.position = p;
        }

        /// <summary>
        /// Create the circle model. 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="segments"></param>
        /// <param name="width"></param>
        private void CreateCircle(LineRenderer line, int segments, double width)
        {
            double step = ((Math.PI * 2) / ((float)segments - 1));
            for (int i = 0; i < segments; ++i)
            {
                line.SetPosition(i,
                    new Vector3(
                    (float)(Math.Cos(step * i) * width),
                    -0.1f,
                    (float)(Math.Sin(step * i) * width)
                ));
            }
        }

        /// <summary>
        /// Create a selection model for a specific kerbal.
        /// </summary>
        /// <param name="eva"></param>
        private void CreateLine(EvaContainer container)
        {
            if (selectionLines.ContainsKey(container.flightID))
            {
                return;
            }

            LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>();

            lineRenderer.useWorldSpace = false;
            lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            //lineRenderer.SetColors(Color.green, Color.red);
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.red;

			Renderer _renderer = null;
			lineRenderer.GetComponentCached<Renderer> (ref _renderer);

			if (_renderer != null) {
				_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				_renderer.receiveShadows = false;
			}

            int segments = 32;

            lineRenderer.positionCount = (segments);

            CreateCircle(lineRenderer, segments, 0.25);

            //set properties
            SetSelectionLineProperties(container.EVA, lineRenderer);

            selectionLines.Add(container.flightID, lineRenderer);

        }

        /// <summary>
        /// Destroy the selection model for a specific kerbal.
        /// </summary>
        /// <param name="eva"></param>
        private void DestroyLine(Guid flightID)
        {
            if (selectionLines.ContainsKey(flightID))
            {
                Destroy(selectionLines[flightID]);//destroy object
                selectionLines.Remove(flightID); // and throw away the key.
            }
        }


        /// <summary>
        /// Enable the cursor for rendering.
        /// </summary>
        private void ShowCursor()
        {
            showCursor = true;
			Renderer _renderer = null;
			_cursor.GetComponentCached<Renderer> (ref _renderer);
            _renderer.enabled = false;
        }

        /// <summary>
        /// Disable the cursor for rendering.
        /// </summary>
        private void DisableCursor()
        {
            showCursor = false;
			Renderer _renderer = null;
			_cursor.GetComponentCached<Renderer> (ref _renderer);
            _renderer.enabled = false;

            //_cursor.SetColors(Color.green, Color.green);
            _cursor.startColor = _cursor.endColor = Color.green;
            _animatedCursor = false;
            _animatedCursorValue = 0;
        }

        /// <summary>
        /// Draw the selection box.
        /// </summary>
        private void OnGUI()
        {
            if (gameUIToggle)
            {
				var visibleBox = _selection.width != 0 && _selection.height != 0;
				var noWindow = GUIUtility.hotControl == 0;
					
				if (_startClick != -Vector3.one && visibleBox && noWindow)
                {
                    GUI.color = new Color(1, 1, 1, 0.15f);
                    GUI.DrawTexture(_selection, _selectionHighlight);
                }
            }
        }

        void GameUIEnable()
        {
            gameUIToggle = true;
        }

        void GameUIDisable()
        {
            gameUIToggle = false;
        }


        public void Update()
        {
            if (!FlightGlobals.ready || PauseMenu.isOpen)
                return;
            
            try
            {
                angle += 0.1;

                #region Update selected kerbals
                foreach (EvaContainer eva in EvaController.instance.collection)
                {
                    if (!eva.Loaded)
                        continue;

                    if (eva.Selected)
                    {
                        UpdateSelectionLine(eva);
                    }
                }
                #endregion

          
                if (!FlightGlobals.ActiveVessel.Landed && FlightGlobals.ActiveVessel.GetHeightFromSurface() > 25)
                {
                    DisableCursor();
                    return;
                }

                if (HighLogic.LoadedScene != GameScenes.FLIGHT || MapView.MapIsEnabled)
                    return;

                //add here something to change the selection ui in space.
                

                #region Handle Cursor...
                if (showCursor)
                {
                    if (!_animatedCursor)
                    {
                        //ray every time ?
                        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _cursorHit))
                        {
                            _cursorPosition = _cursorHit.point;
                            _cursorRotation = FlightGlobals.ActiveVessel.transform.rotation;
                        }
                    }

                    SetCursorProperties();
                }
                #endregion

                #region Select Multiple Kerbals

                if (Input.GetMouseButtonDown(EvaSettings.selectMouseButton)
                    || Input.GetKeyDown(EvaSettings.selectKeyButton))
                {
                    _startClick = Input.mousePosition;
                }
                else if (Input.GetMouseButtonUp(EvaSettings.selectMouseButton)
                    || Input.GetKeyUp(EvaSettings.selectKeyButton))
                {
                    if (_selection.width < 0)
                    {
                        _selection.x += _selection.width;
                        _selection.width = -_selection.width;
                    }
                    if (_selection.height < 0)
                    {
                        _selection.y += _selection.height;
                        _selection.height = -_selection.height;
                    }
						
                    _startClick = -Vector3.one;
                }

                if (Input.GetMouseButton(EvaSettings.selectMouseButton) 
                    || Input.GetKey(EvaSettings.selectKeyButton))
                {
                    _selection = new Rect(_startClick.x, InvertY(_startClick.y),
                        Input.mousePosition.x - _startClick.x, InvertY(Input.mousePosition.y) - InvertY(_startClick.y));
                }

                if (Input.GetMouseButton(EvaSettings.selectMouseButton)
                    || Input.GetKey(EvaSettings.selectKeyButton))
                {
                    if (_selection.width != 0 && _selection.height != 0)
                    {
						Rect _temp = new Rect(_selection.x, _selection.y, _selection.width, _selection.height);
						if (_temp.width < 0)
						{
							_temp.x += _temp.width;
							_temp.width = -_temp.width;
						}
						if (_selection.height < 0)
						{
							_temp.y += _temp.height;
							_temp.height = -_temp.height;
						}

                        //get the kerbals in the selection.
                        foreach (EvaContainer container in EvaController.instance.collection)
                        {                            
                            if (!container.Loaded)
                            {
                                //Can't select what isn't there.
                                continue;
                            }

                            Vector3 camPos = Camera.main.WorldToScreenPoint(container.EVA.transform.position);
                            camPos.y = InvertY(camPos.y);

							if (_temp.Contains(camPos))
                            {
                                SelectEva(container);
                            }
                            else
                            {
                                if (container.Selected)
                                    DeselectEva(container);
                            }
                        }
                    }

                    #region targetVesselBySelection
                    if (HighLogic.CurrentGame.Parameters.CustomParams<EvaFollowerMiscSettings>().targetVesselBySelection)
                    {
                        if (_selection.width != 0 && _selection.height != 0)
                        {
                            Vessel target = null;
                            float longest = 0;
                            //Scan a targetable vessel is avaible. 
                            foreach (Vessel vessel in FlightGlobals.Vessels)
                            {
                                if (!vessel.loaded)
                                    return;

								var camera = GetComponent<Camera>();

                                //Calculate distance.
                                var distance = Mathf.Abs(
									Vector3.Distance(vessel.GetWorldPos3D(), camera.transform.position));

                                if (target == null)
                                {
                                    longest = distance;
                                    target = vessel;
                                }
                                else
                                {
                                    if (distance > longest)
                                    {
                                        longest = distance;
                                        target = vessel;
                                    }
                                }
                            }

                            if (target != null)
                            {
                                Vector3 camPos = Camera.main.WorldToScreenPoint(target.transform.position);
                                camPos.y = InvertY(camPos.y);

                                if (_selection.Contains(camPos))
                                {
                                    //target the vessel.  
                                    FlightGlobals.fetch.SetVesselTarget(target);
                                }
                            }
                        }
                    }

                    #endregion
                }
                #endregion

                              

                #region Select Single Kerbal

                bool leftButton = Input.GetMouseButtonDown(EvaSettings.selectMouseButton) || Input.GetKeyDown(EvaSettings.selectKeyButton);
                bool rightButton = Input.GetMouseButtonDown(EvaSettings.dispatchMouseButton) || Input.GetKeyDown(EvaSettings.dispatchKeyButton);

                if (leftButton == false && rightButton == false)
                {
                    return;
                }

                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

                if (!hit)
                {
                    DisableCursor();
                    return; //nothing to check.
                }

                var evaCollision = hitInfo.transform.gameObject.GetComponent<KerbalEVA>();

                if (leftButton)
                {
                    DeselectAllKerbals();

                    if (evaCollision != null)
                    {
                        EvaContainer eva = EvaController.instance.GetEva(evaCollision.vessel.id);

                        if (!eva.Loaded)
                        {
                            throw new Exception("[EFX] Impossibre!"); // NO_LOCALIZATION
                        }

                        SelectEva(eva);
                    }
                    else
                    {
                        DisableCursor();
                    }
                }
                #endregion

                #region Handle Mouse Controls
                if (rightButton) //Middle button.
                {

                    var offset = (FlightGlobals.ActiveVessel).GetWorldPos3D();
                    var position = (Vector3d)hitInfo.point;

                    foreach (var item in EvaController.instance.collection.ToArray())
                    {
                        if (!item.Loaded)
                            return;

                        if (item.Selected)
                        {
                            //Remove current mode.
                            if (item.mode == Mode.Patrol)
                            {
                                item.EndPatrol();
                            }
                            
                            if (EvaSettings.displayDebugLines)
                            {
                                setLine(position, offset);
                            }

                            EvaDebug.DebugLog(string.Format("Target: {0}", position));

                            item.Order(position, offset);
                            item.Selected = false;
                            item.mode = Mode.Order;

                            _animatedCursor = true;

                            //destroy circle line
                            DestroyLine(item.flightID);
                        }
                    }
                }
                #endregion

                #region Cursor Visible...
                //Show the cursor if more than one kerbal is selected. 
                if (selectedKerbals > 0)
                {
                    ShowCursor();
                }
                else
                {
                    DisableCursor();
                }
                #endregion

            }
            catch (Exception exp)
            {
                EvaDebug.DebugWarning("[EFX] EvaOrderController: " + exp.Message); // NO_LOCALIZATION
            }
        }

        private void DeselectAllKerbals()
        {
            //deselect all kerbals.
            foreach (EvaContainer eva in EvaController.instance.collection)
            {
                if (!eva.Loaded)
                    continue;

                if (eva.Selected)
                    DeselectEva(eva);
            }
        }

        
        /// <summary>
        /// Select an EVA, and add the selection to the line collection.
        /// </summary>
        /// <param name="_eva"></param>
        private void SelectEva(EvaContainer container)
        {
			if (!container.EVA.vessel.LandedOrSplashed) {
				return;
			}

            ++selectedKerbals;
            container.Selected = true;

            //create circle line
            CreateLine(container);
        }

        /// <summary>
        /// Deselect an EVA, and remove the selection from the line collection.
        /// </summary>
        /// <param name="_eva"></param>
        private void DeselectEva(EvaContainer _eva)
        {
            --selectedKerbals;
            _eva.Selected = false;

            //create circle line
            DestroyLine(_eva.flightID);
        }


        private float InvertY(float y)
        {
            return Screen.height - y;
        }


        LineRenderer debugLine;

        private void setLine(Vector3d position, Vector3d target)
        {
            debugLine.positionCount= (2);
            debugLine.SetPosition(0, position);
            debugLine.SetPosition(1, target);
        }

        private void InitializeDebugLine()
        {
            debugLine = new GameObject().AddComponent<LineRenderer>();

            debugLine.useWorldSpace = false;
            debugLine.material = new Material(Shader.Find("Particles/Additive"));
            debugLine.startWidth = 0.05f;
            debugLine.endWidth = 0.05f;
            //debugLine.SetColors(Color.green, Color.red);
            debugLine.startColor = Color.green;
            debugLine.endColor = Color.red;

            Renderer _renderer = null;
			debugLine.GetComponentCached<Renderer> (ref _renderer);

			if (_renderer) {
				_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				_renderer.receiveShadows = false;
				_renderer.enabled = true;
			}

            debugLine.positionCount = 0;
        }


    }
}
