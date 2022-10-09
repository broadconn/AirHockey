using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPosOnMesh : MonoBehaviour
{
    [SerializeField] MeshFilter mesh;
    [SerializeField] Transform malletPosition;
    [SerializeField] Transform desiredPos;
    [SerializeField] Transform debugPoint;
    [SerializeField] int numRefinements = 3;

    // Update is called once per frame
    void Update()
    {
        // set numRefinements to 0 to see why this is needed.
        var meshPoint = AIMallet.ClosestPointOnMesh(mesh, desiredPos.position);
        for (int i = 0; i < numRefinements; i++) {
            var vecToMeshPoint = meshPoint - malletPosition.position;
            var distToMeshPoint = vecToMeshPoint.magnitude;
            var desiredPosAtMeshPointDist = malletPosition.position + (desiredPos.position - malletPosition.position).normalized * distToMeshPoint;
            meshPoint = AIMallet.ClosestPointOnMesh(mesh, desiredPosAtMeshPointDist);
        }

        debugPoint.position = meshPoint;
    }
}
