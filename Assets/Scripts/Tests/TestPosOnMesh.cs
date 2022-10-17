using Assets.Scripts.Utility;
using UnityEngine;

public class TestPosOnMesh : MonoBehaviour
{
    [SerializeField] MeshFilter mesh;
    [SerializeField] Transform malletPosition;
    [SerializeField] Transform desiredPos;
    [SerializeField] Transform debugPoint;
    [SerializeField] int numRefinements = 3; // set numRefinements to 0 to see why this is needed.

    // Update is called once per frame
    void Update()
    {  
        var meshPoint = Utilities.GetClosestPointOnMeshAlongLineFromPoint(mesh, desiredPos.position, malletPosition.position, numRefinements); 
        debugPoint.position = meshPoint;
    }
}
