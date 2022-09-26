using System.Collections.Generic;
using System.IO;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets {
    internal class PuckFuturePath: MonoBehaviour {
        Rigidbody rb;

        [SerializeField] bool debugVisualizePath = true;
        [SerializeField] float puckRadius = 0.3f;
        [SerializeField] int futureBounces = 5;
        int futureBouncesLeft = 0;

        [SerializeField] Transform debugDotsParent; 

        public bool WillEnterGoal { get => willEnterGoal; }
        bool willEnterGoal = false;

        readonly List<(Vector3 point, float timeToReach)> futurePath = new();
        int layerMask;

        private void Start() {
            rb = GetComponent<Puck>().Rb;
            layerMask = LayerMask.GetMask("WallCollider", "GoalCollider"); // only simulate bounces on the arena walls and goals
        }

        private void Update() { 
            futurePath.Clear();

            if (rb.velocity.magnitude > 0) 
                DoFutureCast(); 

            if (debugVisualizePath) DrawFuturePathLines();
        }

        // fire a spherecast out from the puck, the size of the puck, only against the wall colliders and the goal.
        // if we hit a wall, start a new spherecast where it hits the wall.  
        void DoFutureCast() {
            willEnterGoal = false;
            var castPos = rb.position;
            var castDir = rb.velocity.normalized;
            var totalPathTime = 0f;
            futureBouncesLeft = futureBounces;

            futurePath.Add((rb.position, 0));

            while (futureBouncesLeft > 0) {
                if (Physics.SphereCast(castPos, puckRadius, castDir, out RaycastHit hit, 100, layerMask)) {
                    var bounceCenter = castPos + (castDir.normalized * hit.distance); // hit.distance is the distance the sphere travelled before it collided                    
                    var lastPathPoint = futurePath[^1].point;
                    var distBetweenPoints = Vector3.Distance(bounceCenter, lastPathPoint);
                    var timeToReachPoint = distBetweenPoints / rb.velocity.magnitude; // assume velocity doesn't change
                    totalPathTime += timeToReachPoint;
                    var futurePoint = (bounceCenter, totalPathTime);

                    futurePath.Add(futurePoint);

                    if (hit.transform.CompareTag("Goal")) {
                        willEnterGoal = true;
                        break;
                    }

                    castPos = bounceCenter;
                    castDir = Vector3.Reflect(castDir, hit.normal);
                }
                else {
                    break;
                }
                futureBouncesLeft--;
            }

            // populate individual direction paths
            foreach(var future in futurePath) {

            }
        }

        //public Vector3 GetClosestPoint(Vector3 samplePoint) {
        //    if (futurePath.Count == 0) {
        //        print("FUTURE IS EMPTY, I CANNOT");
        //        return Vector3.zero;
        //    }

        //    // iterate over all the futurePoints, finding the closest point to the AI
        //    var closestIdx = 0;
        //    var closestDist = Mathf.Infinity;
        //    for (int i = 0; i < futurePath.Count; i++) {
        //        var future = futurePath[i];
        //        var dist = Vector3.Distance(samplePoint, future.point);
        //        if (dist < closestDist) {
        //            closestIdx = i;
        //            closestDist = dist;
        //        }
        //    }

        //    // get the <=2 segments around that point
        //    var segments = new List<(Vector3, Vector3)>();
        //    // if the index is 0 we also have to take into account the line from the player to the 
        //    if (closestIdx == 0) {
        //        segments.Add(new(futurePath[closestIdx].point, futurePath[closestIdx - 1].point));
        //    }
        //    else {
        //        if (closestIdx > 0)
        //            segments.Add(new(futurePath[closestIdx].point, futurePath[closestIdx - 1].point));
        //        if (closestIdx < futurePath.Count - 1)
        //            segments.Add(new(futurePath[closestIdx].point, futurePath[closestIdx + 1].point));
        //    }


        //    // search the segments for the closest in-between location to the AI


        //    // return the closest point.

        //}

        void DrawFuturePathLines() {
            // reset dots + line
            for (int i = 0; i < debugDotsParent.childCount; i++) {
                debugDotsParent.GetChild(i).transform.localPosition = Vector3.zero;
                debugDotsParent.GetChild(i).GetComponent<LineRenderer>().SetPositions(new Vector3[2] { Vector3.zero, Vector3.zero });
            } 

            // draw dots along the path points + lines
            for(int i = 0; i < futurePath.Count; i++) {
                var debugPointIdx = i * 2;
                if (debugPointIdx < debugDotsParent.childCount) {
                    var debugDot = debugDotsParent.GetChild(debugPointIdx);
                    var pathPoint = futurePath[i].point;
                    debugDot.transform.position = pathPoint;

                    var prevPoint = i == 0 ? rb.position : futurePath[i - 1].point;
                    var points = new Vector3[2] { pathPoint, prevPoint }; 
                    debugDot.GetComponent<LineRenderer>().SetPositions(points);
                }
            }
        }
    }
}
