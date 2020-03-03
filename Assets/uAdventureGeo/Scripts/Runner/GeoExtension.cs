﻿using UnityEngine;
using System.Collections;
using AssetPackage;
using uAdventure.Runner;
using uAdventure.Core;

namespace uAdventure.Geo
{
    public class GeoExtension : GameExtension {

        private static GeoExtension instance;
        public static GeoExtension Instance {
            get
            {
                return instance;
            }
        }

        public bool UsingDebugLocation
        {
            get { return memory.Get<bool>("using_debug_location"); }
            set
            {
                if ((Application.isEditor || PreviewManager.Instance.InPreviewMode) && Application.isPlaying)
                {
                    memory.Set("using_debug_location", value);
                }
            }
        }


        public GeoPositionedCharacter geochar;
        public float timeToFlush = 5;
        public Texture2D connectedSimbol;
        public Texture2D connectingSimbol;
        public Texture2D disconnectedSimbol;
        public float blinkingTime;
        public float iconWidth, iconHeight;
        private Memory memory;
        private float timeSinceLastPositionUpdate = 0;

        private float time;

        public float update = .1f; // 5 meters
        public float accuracy = 1; // 10 meters
        private bool inMapScene = false;
        private bool inZoneControl = false;
        private bool hidden = false;
        private GUIMap guiMap;
        private Rect debugWindowRect = new Rect(0, 0, Application.isMobilePlatform ? 300 : 200, Application.isMobilePlatform ? 300 : 200);
        private Texture2D pointer;

        void Awake()
        {
            instance = this;
            Restart();
        }

        public void Start()
        {
            if (!PreviewManager.Instance.InPreviewMode && !IsStarted())
            {
                StartCoroutine(StartLocation());
            }

            if (Application.isPlaying)
            {
                Game.TargetChangedDelegate checkTarget = (newTarget) =>
                {
                    inMapScene = newTarget is MapScene;
                    inZoneControl = GameObject.FindObjectOfType<ZoneControl>();
                };

                Game.Instance.OnTargetChanged += checkTarget;
                checkTarget(Game.Instance.GameState.GetChapterTarget(Game.Instance.GameState.CurrentTarget));
            }

            pointer = Resources.Load<Texture2D>("pointer");
        }

        public override void Restart()
        {
            memory = new Memory();
            memory.Set("using_debug_location", false);
            memory.Set("debug_location", Vector2d.zero);
            memory.Set("navigating", 0);
            memory.Set("zone_control", false);
            Game.Instance.GameState.SetMemory("geo_extension", memory);
            CreateNavigationAndZoneControl();
        }

        public override void OnGameReady()
        {
            this.memory = Game.Instance.GameState.GetMemory("geo_extension") ?? memory;
            CreateNavigationAndZoneControl();
        }

        public override void OnBeforeGameSave() { }
        public override void OnAfterGameLoad() { }

        private void CreateNavigationAndZoneControl()
        {
            var oldNavigation = FindObjectOfType<NavigationController>();
            if (oldNavigation)
            {
                DestroyImmediate(oldNavigation.gameObject);
            }

            // In case is necesary
            if (memory.Get<bool>("navigating"))
            {
                var newNavigation = Instantiate(Resources.Load<GameObject>("navigation"));
                newNavigation.GetComponent<NavigationController>().RestoreNavigation(memory);
            }

            var oldZoneControl = FindObjectOfType<ZoneControl>();
            if (oldZoneControl)
            {
                DestroyImmediate(oldZoneControl.gameObject);
            }

            if (memory.Get<bool>("zone_control") && !FindObjectOfType<ZoneControl>())
            {
                var newZoneControl = new GameObject("zone_control");
                newZoneControl.AddComponent<ZoneControl>().Restore(memory);
            }
        }


        IEnumerator StartLocation()
        {
            // First, check if user has location service enabled
            if (!Input.location.isEnabledByUser)
                yield break;

            // Start service before querying location
            Input.location.Start(accuracy, update);

            // Wait until service initializes
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            // Service didn't initialize in 20 seconds
            if (maxWait < 1)
            {
                print("Timed out");
                yield break;
            }

            // Connection has failed
            if (Input.location.status == LocationServiceStatus.Failed)
            {
                print("Unable to determine device location");
                yield break;
            }

            // Access granted and location value could be retrieved
            print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

        }

        public bool IsStarted()
        {
            return Input.location.status == LocationServiceStatus.Initializing || Input.location.status == LocationServiceStatus.Running;
        }

        void Update()
        {
            if (!Application.isEditor && !PreviewManager.Instance.InPreviewMode && !IsStarted())
            {
                StartCoroutine(StartLocation());
            }

            time += Time.deltaTime;
            timeSinceLastPositionUpdate += Time.deltaTime;

            if (time > blinkingTime)
            {
                time -= Mathf.Floor(time / blinkingTime) * blinkingTime;
            }

            if (Game.Instance.CurrentTargetRunner.Data is MapScene && !geochar)
            {
                geochar = FindObjectOfType<GeoPositionedCharacter>();
            }

            if (timeSinceLastPositionUpdate > timeToFlush)
            {
                timeSinceLastPositionUpdate = 0;

                if (IsStarted() || UsingDebugLocation || geochar)
                {
                    var mapScene = Game.Instance.CurrentTargetRunner.Data as MapScene;
                    if (mapScene != null)
                    {
                        TrackerExtension.Movement.Moved(mapScene.Id, Location);
                    }
                    else
                    {
                        TrackerExtension.Movement.Moved("World", Location);
                    }

                    TrackerAsset.Instance.Flush();
                }
            }

        }

        void OnGUI()
        {
            var paintSimbol = disconnectedSimbol;

            switch (Input.location.status) {
                default:
                case LocationServiceStatus.Failed:
                case LocationServiceStatus.Stopped:
                    paintSimbol = disconnectedSimbol;
                    break;
                case LocationServiceStatus.Initializing:
                    paintSimbol = connectingSimbol;
                    break;
                case LocationServiceStatus.Running:
                    var connecting = ((time > blinkingTime / 2f) ? connectedSimbol : connectingSimbol);
                    paintSimbol = IsLocationValid() ? connectedSimbol : connecting;
                    break;
            }

            if (paintSimbol && Event.current.type == EventType.Repaint)
            {
                GUI.DrawTexture(new Rect(Screen.width - iconWidth - 5, 5, iconWidth, iconHeight), paintSimbol);
            }

            if (Application.isEditor || PreviewManager.Instance.InPreviewMode)
            {
                if (guiMap == null)
                {
                    guiMap = new GUIMap();
                }

                if (Input.GetKeyDown(KeyCode.G))
                {
                    hidden = !hidden;
                }

                if (!hidden && (inMapScene || inZoneControl))
                {
                    debugWindowRect = GUI.Window(12341234, debugWindowRect, (id) =>
                    {
                        var scale = Application.isMobilePlatform ? 1.5f : 1f;
                        var mapRect = new Rect(2, 18, 196 * scale, 180 * scale);
                        using (new GUILayout.AreaScope(mapRect))
                        {
                            guiMap.Center = GeoExtension.Instance.Location;
                            guiMap.Zoom = 17;
                            guiMap.DrawMap(new Rect(0, 0, 196 * scale, 180 * scale));
                            // Calculate the player pixel relative to the map
                            var playerMeters = MapzenGo.Helpers.GM.LatLonToMeters(GeoExtension.Instance.Location);
                            var playerPixel = MapzenGo.Helpers.GM.MetersToPixels(playerMeters, guiMap.Zoom);
                            var playerPixelRelative = playerPixel + guiMap.PATR;

                            // Do the point handling
                            var pointControl = GUIUtility.GetControlID("PlayerPosition".GetHashCode(), FocusType.Passive);
                            var oldPlayerPixel = playerPixelRelative.ToVector2();
                            var newPlayerPixel = HandlePointMovement(pointControl, oldPlayerPixel, 60 * scale,
                                (point, isOver, isActive) =>
                                {
                                    var locationRect = new Rect(0, 0, 30 * scale, 30 * scale);
                                    locationRect.center = point;
                                    locationRect.y -= locationRect.height / 2f;
                                    GUI.DrawTexture(locationRect, pointer);
                                });
                            if (oldPlayerPixel != newPlayerPixel)
                            {
                                if (Application.isMobilePlatform)
                                {
                                    var ydif = newPlayerPixel.y - oldPlayerPixel.y;
                                    newPlayerPixel.y -= 2 * ydif;
                                }

                                // If changed, restore the point to the geochar
                                playerPixel = newPlayerPixel.ToVector2d() - guiMap.PATR;
                                playerMeters = MapzenGo.Helpers.GM.PixelsToMeters(playerPixel, guiMap.Zoom);
                                GeoExtension.Instance.Location = MapzenGo.Helpers.GM.MetersToLatLon(playerMeters);
                            }

                            guiMap.ProcessEvents(mapRect);

                            GUI.Label(new Rect(0, 0, 196 * scale, 40 * scale), "Drag the pointer to move");
                        }
                        GUI.DragWindow();
                    },
                       "Simulated Location");
                }
            }
        }

        public bool IsLocationValid()
        {
            return UsingDebugLocation || (Input.location.status == LocationServiceStatus.Running
                && Input.location.lastData.timestamp > 0 
                && Input.location.lastData.LatLon() != Vector2.zero
                && Mathf.Max(Input.location.lastData.horizontalAccuracy, Input.location.lastData.verticalAccuracy) < 50); // Max 50 metros
        }

        public Vector2d Location
        {
            get
            {
                if (UsingDebugLocation)
                {
                    return memory.Get<Vector2d>("debug_location");
                }

                if (IsLocationValid())
                {
                    return Input.location.lastData.LatLonD();
                }

                if (geochar)
                {
                    return geochar.LatLon;
                }

                return Vector2d.zero;
            }
            set
            {
                if ((Application.isEditor || PreviewManager.Instance.InPreviewMode) && Application.isPlaying)
                {
                    UsingDebugLocation = true;
                    memory.Set("debug_location", value);
                }
            }
        }



        private static Vector2 HandlePointMovement(int controlId, Vector2 point, float maxDistance, System.Action<Vector2, bool, bool> draw)
        {
            var cursorRect = new Rect(0, 0, maxDistance * 1.5f, maxDistance * 1.5f) { center = point };

            var isOver = (point - Event.current.mousePosition).magnitude < maxDistance;
            var isActive = GUIUtility.hotControl == controlId;

            switch (Event.current.type)
            {
                case EventType.Repaint: draw(point, isOver, isActive); break;
                case EventType.MouseDown:
                    if (isOver)
                    {
                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (isActive)
                    {
                        point.x += Event.current.delta.x;
                        point.y += Event.current.delta.y;
                        GUI.changed = true;
                        Event.current.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (isActive)
                    {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }
                    break;
            }

            return point;
        }
    }
}