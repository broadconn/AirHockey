using System.Collections.Generic;
using System.IO;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Assets {
    internal class PuckFuturePath: MonoBehaviour {
        Rigidbody rb;

        [SerializeField] bool debugVisualizePath = true;
        [SerializeField] float puckRadius = 0.3f;
        [SerializeField] float futureDistance = 10;

        [SerializeField] Transform debugDotsParent;

        const int wallCollidersLayer = 7;

        public bool WillEnterGoal { get => willEnterGoal; }
        bool willEnterGoal = false;

        readonly List<Vector3> pathPoints = new(); // TODO: include time to reach each point, to help the AI know where to intercept-----------------------------------------------------------------


        private void Start() {
            rb = GetComponent<Puck>().Rb;
        }

        private void Update() {
            pathPoints.Clear();

            if (rb.velocity.magnitude > 0) 
                DoFutureCast(); 

            if (debugVisualizePath) DrawFuturePathLines();
        }

        // fire a spherecast out from the puck, the size of the puck, only against the wall colliders and the goal.
        // if we hit a wall, start a new spherecast where it hits the wall.  
        void DoFutureCast() {
            willEnterGoal = false;
            var futurePathLeft = futureDistance;
            var castOrigin = rb.position;
            var castDir = rb.velocity.normalized;

            while (futurePathLeft > 0) {
                //cast out 
                int layerMask = 1 << wallCollidersLayer; // only cast rays against the arena walls
                if (Physics.SphereCast(castOrigin, puckRadius, castDir, out RaycastHit hit, 100, layerMask)) {
                    // we hit a wall, so save this point...
                    var hitCenter = castOrigin + (castDir.normalized * hit.distance); // apparently the hit.distance is actually the distance the sphere travelled before it collided                    
                    pathPoints.Add(hitCenter);
                    castOrigin = hitCenter;

                    // dont keep bouncing if we hit a goal.
                    if (hit.transform.CompareTag("Goal")) {
                        willEnterGoal = true;
                        break;
                    }

                    // ...otherwise set the next cast to be in the bounce direction. 
                    castDir = Vector3.Reflect(castDir, hit.normal);

                    futurePathLeft -= hit.distance;
                }
                else {
                    break;
                }
            }
        }

        void DrawFuturePathLines() {
            // reset dots + line
            for (int i = 0; i < debugDotsParent.childCount; i++) {
                debugDotsParent.GetChild(i).transform.localPosition = Vector3.zero;
                debugDotsParent.GetChild(i).GetComponent<LineRenderer>().SetPositions(new Vector3[2] { Vector3.zero, Vector3.zero });
            } 

            // draw dots along the path points + lines
            for(int i = 0; i < pathPoints.Count; i++) {
                var debugPointIdx = i * 2;
                if (debugPointIdx < debugDotsParent.childCount) {
                    var debugDot = debugDotsParent.GetChild(debugPointIdx);
                    var pathPoint = pathPoints[i];
                    debugDot.transform.position = new Vector3(pathPoint.x, pathPoint.y, pathPoint.z);

                    var prevPoint = i == 0 ? rb.position : pathPoints[i - 1];
                    var points = new Vector3[2] { pathPoint, prevPoint }; 
                    debugDot.GetComponent<LineRenderer>().SetPositions(points);
                }
            }
        }
    }
}
