using Assets.Scripts.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Assets {
    internal class PuckFuturePath: MonoBehaviour {
        Rigidbody rb;

        [SerializeField] float puckRadius = 0.3f;
        [SerializeField] int futureBounces = 5;
        int numBouncesLeft = 0;

        [Header("Debug")]
        [SerializeField] Transform debugClosestPoint;
        [SerializeField] Transform debugChosenPoint;
        [SerializeField] Transform debugDotsParent;
        [SerializeField] AIMallet aiMallet; 

        public bool WillEnterGoal { get => willEnterGoal; }
        bool willEnterGoal = false;

        public Vector3? RedWallImpactPoint;

        readonly List<((Vector3 point, float timeToReach) pointA, (Vector3 point, float timeToReach) pointB)> futureSegments = new(); //lol

        int layerMask;

        private void Start() {
            rb = GetComponent<Puck>().Rb;
            layerMask = LayerMask.GetMask("WallCollider", "GoalCollider"); // only simulate bounces on the arena walls and goals
        }

        private void Update() { 
            DoFutureCastAgainstRed();

            if (GameController.Instance.debug) {
                DrawFuturePathLines();
                ShowClosestPoint();
                ShowChosenPoint();
            }
        }

        // fire a spherecast out from the puck, the size of the puck, only against the wall colliders and the goal.
        // if we hit a wall, start a new spherecast where it hits the wall.  
        void DoFutureCastAgainstRed() {
            // reset values
            futureSegments.Clear();
            willEnterGoal = false;
            var castPos = rb.position;
            var castDir = rb.velocity.normalized;
            var totalPathTime = 0f;
            RedWallImpactPoint = null;

            if (rb.velocity.magnitude <= 0) return; // puck not moving

            var prevPoint = (rb.position, 0f); 
            numBouncesLeft = futureBounces;
            while (numBouncesLeft > 0) {
                if (Physics.SphereCast(castPos, puckRadius, castDir, out RaycastHit hit, Mathf.Infinity, layerMask)) {
                    var bounceCenter = castPos + (castDir.normalized * hit.distance); // hit.distance is the distance the sphere travelled before it collided                    
                    
                    // path traversal time 
                    var distBetweenPoints = Vector3.Distance(bounceCenter, prevPoint.position);
                    var timeToReachPoint = distBetweenPoints / rb.velocity.magnitude; // assume velocity doesn't change
                    totalPathTime += timeToReachPoint;

                    // path bounce point
                    var futurePoint = (bounceCenter, totalPathTime); 
                    futureSegments.Add((prevPoint, futurePoint));
                    prevPoint = futurePoint;

                    // stop if we enter the goal
                    if (hit.transform.CompareTag("Goal")) {
                        willEnterGoal = true;
                        break;
                    }

                    // set values for next raycast
                    castPos = bounceCenter;
                    var newCastDir = Vector3.Reflect(castDir, hit.normal);

                    //check for red wall impact
                    if (RedWallImpactPoint is null)
                        if (Mathf.Sign(castDir.z) > Mathf.Sign(newCastDir.z)) {
                            RedWallImpactPoint = bounceCenter;
                        }

                    castDir = newCastDir;
                }
                numBouncesLeft--;
            } 
        }

        void ShowClosestPoint() {
            var closestPt = GetClosestPointOnPath(aiMallet.transform.position);
            debugClosestPoint.position = closestPt.point;
        }

        void ShowChosenPoint() {
            var chosenPoint = aiMallet.GetDestination();
            debugChosenPoint.position = chosenPoint + new Vector3(0, 0.05f, 0);
        }
         
        public (Vector3 point, float timePuckReachesHere) GetClosestPointOnPath(Vector3 samplePoint) {
            if (futureSegments.Count == 0)  
                return (Vector3.zero, 0);  

            // search the segments for the closest in-between location to the sample point
            var closestDist2 = Mathf.Infinity;
            var closestPos = Vector3.zero; 
            var closestSegment = futureSegments[0];
            bool lastSegIsTowardsRedZone = false;
            foreach (var seg in futureSegments) {
                var segmentDirection = seg.pointB.point - seg.pointA.point;

                // if we have finished going through a red-zone-heading section of segments, stop looking. 
                // this is a side effect / optimisation :D
                var thisSegIsTowardsRedZone = segmentDirection.z > 0;
                if (lastSegIsTowardsRedZone && !thisSegIsTowardsRedZone)
                    break;

                lastSegIsTowardsRedZone = thisSegIsTowardsRedZone;

                var pos = GetClosestPointOnLine((seg.pointA.point, seg.pointB.point), samplePoint);
                var dist = Vector3.Distance(samplePoint, pos);
                if (dist < closestDist2) {
                    closestDist2 = dist;
                    closestPos = pos;
                    closestSegment = seg;
                }
            }

            // for the time, get the position as a percentage between the segment points and lerp between the segment point times
            var percTime = Utilities.InverseLerp(closestSegment.pointA.point, closestSegment.pointB.point, closestPos);
            var time = Mathf.Lerp(closestSegment.pointA.timeToReach, closestSegment.pointB.timeToReach, percTime);

            // return the closest point.
            return (closestPos, time);
        }

        private static Vector3 GetClosestPointOnLine((Vector3 start, Vector3 end) line, Vector3 point) {
            var startToPoint = point - line.start;        
            var startToEnd = line.end - line.start;      

            var lineSqLength = Vector3.SqrMagnitude(startToEnd);
            var dotABAP = Vector3.Dot(startToPoint, startToEnd); 
            var distance = dotABAP / lineSqLength;
            var pointOnLine = 
                distance < 0 ? line.start : 
                distance > 1 ? line.end : 
                line.start + startToEnd * distance;

            return pointOnLine;
        }

        void DrawFuturePathLines() {
            // reset dots + line
            for (int i = 0; i < debugDotsParent.childCount; i++) {
                debugDotsParent.GetChild(i).transform.localPosition = Vector3.zero;
                debugDotsParent.GetChild(i).GetComponent<LineRenderer>().SetPositions(new Vector3[2] { Vector3.zero, Vector3.zero });
            } 

            // draw dots along the path points + lines
            for(int i = 0; i < futureSegments.Count; i++) {
                var debugPointIdx = i * 2;
                if (debugPointIdx < debugDotsParent.childCount) {
                    var debugDot = debugDotsParent.GetChild(debugPointIdx);
                    var pathPoint = futureSegments[i].pointB.point;
                    debugDot.transform.position = pathPoint;

                    var prevPoint = futureSegments[i].pointA.point;
                    var points = new Vector3[2] { pathPoint, prevPoint }; 
                    debugDot.GetComponent<LineRenderer>().SetPositions(points); // would be better to save the line renderers, but its debug so whatever
                }
            }
        }
    }
}
