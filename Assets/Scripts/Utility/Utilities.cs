using UnityEngine;

namespace Assets.Scripts.Utility {
    public static class Utilities {
        /// <summary>
        /// Limits the <paramref name="toPoint"/> to the <paramref name="meshFilter"/>, along the line between the <paramref name="toPoint"/> and the <paramref name="fromPoint"/>.  <br></br>
        /// See the test scene to highlight why refinements are required, but if the <paramref name="meshFilter"/> is square then the closest point on the mesh likely won't be on the line between the <paramref name="toPoint"/> and the <paramref name="fromPoint"/>. <br></br> 
        /// If we sample again along the desired line at a distance that the first sample reached it will be very close to the mesh edge, which will be more accurate.
        /// Do this more times to increase accuracy.
        /// </summary>
        /// <param name="meshFilter"></param>
        /// <param name="toPoint"></param>
        /// <param name="fromPoint"></param>
        /// <param name="numRefinements"></param>
        /// <returns></returns>
        public static Vector3 GetClosestPointOnMeshAlongLineFromPoint(MeshFilter meshFilter, Vector3 toPoint, Vector3 fromPoint, int numRefinements = 2) {
            var closeMeshPoint = ClosestPointOnMesh(meshFilter, toPoint);

            for (int i = 0; i < numRefinements; i++) {
                var tgtPointToMeshPointVector = closeMeshPoint - toPoint;
                var distToMeshPoint = tgtPointToMeshPointVector.magnitude;
                var malletToWorldPointVector = fromPoint - toPoint;
                var closerSamplePoint = toPoint + malletToWorldPointVector.normalized * distToMeshPoint;
                closeMeshPoint = ClosestPointOnMesh(meshFilter, closerSamplePoint);
            }

            return closeMeshPoint;
        }

        public static Vector3 ClosestPointOnMesh(MeshFilter meshFilter, Vector3 worldPoint) {
            var localPoint = meshFilter.transform.InverseTransformPoint(worldPoint);
            var localClosest = meshFilter.sharedMesh.bounds.ClosestPoint(localPoint);
            return meshFilter.transform.TransformPoint(localClosest);
        }

        /// <summary>
        /// The percentage that point p is between a and b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 p) {
            Vector3 AB = b - a;
            Vector3 AP = p - a;
            return Vector3.Dot(AP, AB) / Vector3.Dot(AB, AB);
        }
    }
}
