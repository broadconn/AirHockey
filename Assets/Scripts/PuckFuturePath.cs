using System.Collections.Generic;
using UnityEngine;

namespace Assets {
    internal class PuckFuturePath: MonoBehaviour {
        Rigidbody rb;

        [SerializeField] float puckRadius = 0.3f;
        [SerializeField] int futureBounces = 5;
        int futureBouncesLeft = 0;

        [Header("Debug")]
        [SerializeField] bool debugVisualizePath = true;
        [SerializeField] Transform debugClosestPoint;
        [SerializeField] Transform debugDotsParent;
        [SerializeField] Transform aiMallet; 

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

            if (debugVisualizePath) {
                DrawFuturePathLines();
                ShowClosestPoint();
            }
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
        }

        void ShowClosestPoint() {
            var closestPt = GetClosestPoint(aiMallet.position);
            debugClosestPoint.position = closestPt;
        }
         
        public Vector3 GetClosestPoint(Vector3 samplePoint) {
            if (futurePath.Count == 0) {
                return new Vector3(0, -1000, -1000);
            }

            // iterate over all the future points, finding the closest point to the AI
            var closestDist = Mathf.Infinity;
            var futureSegments = new List<(Vector3 pointA, Vector3 pointB, bool headingTowardsRedZone)>();
            for (int i = 0; i < futurePath.Count; i++) {
                var future = futurePath[i];
                var dist = Vector3.Distance(samplePoint, future.point);
                if (dist < closestDist)  
                    closestDist = dist; 

                if (i > 0) {
                    var prevPoint = futurePath[i - 1].point;
                    var diff = future.point - prevPoint;
                    var towardsRedZone = diff.z > 0;
                    futureSegments.Add((prevPoint, future.point, towardsRedZone));
                }
            }

            // TODO: MAYBE SPLIT THIS UP TO CHECK THE FIRST BATCH OF POINTS THAT ENTER THE ENEMY AREA
            //  THEN EVALUATE IF THE AI CAN INTERCEPT THAT CLOSEST POINT,
            //  IF NOT THEN CHECK THE SECOND BATCH+ (IF THERE IS ONE) OF SEGMENTS THAT ENTER THE ENEMY AREA

            // search the segments for the closest in-between location to the AI
            var closestDist2 = Mathf.Infinity;
            var closestPos = Vector3.zero;
            foreach(var s in futureSegments) {
                if (!s.headingTowardsRedZone) continue;
                var pos = GetClosestPointOnLineAB(s.pointA, s.pointB, samplePoint);
                var dist = Vector3.Distance(samplePoint, pos);
                if (dist < closestDist2) {
                    closestDist2 = dist;
                    closestPos = pos;
                }
            }

            // return the closest point.
            return closestPos;
        }

        private Vector3 GetClosestPointOnLineAB(Vector3 a, Vector3 b, Vector3 point) {
            Vector3 a_to_p = point - a;        
            Vector3 a_to_b = b - a;      

            float lineSqLength = Vector3.SqrMagnitude(a_to_b);
            float dotABAP = Vector3.Dot(a_to_p, a_to_b); 
            float distance = dotABAP / lineSqLength;
            var pointOnLine = 
                distance < 0 ? a : 
                distance > 1 ? b : 
                a + a_to_b * distance;

            return pointOnLine;
        }

        public Vector3? GetClosestInterceptablePoint(Vector3 aiPosition, Vector3 aiMaxVelocity) {
            // TODO
            return null;
        }

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
