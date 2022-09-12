using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMallet : MonoBehaviour
{
    [SerializeField] new Camera camera;
    [SerializeField] MeshFilter malletArea;
    [SerializeField] int malletSpeed = 40;
    Rigidbody rb;
    Vector3 tgtPos;
    const int mouseAreaColliderLayer = 6;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false; 
        rb = GetComponent<Rigidbody>();
        tgtPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() {
        UpdateTgtPos();
        rb.MovePosition(transform.position + malletSpeed * Time.deltaTime * (tgtPos - transform.position));
    }

    void UpdateTgtPos() {
        int layerMask = 1 << mouseAreaColliderLayer; // only cast rays against the P1 mouse area collider
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 10, layerMask)) {
            Vector3 hitWorldPos = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            tgtPos = ClosestPointOnMeshOBB(malletArea, hitWorldPos);
            Debug.DrawRay(camera.transform.position, (tgtPos - camera.transform.position) * 100, Color.yellow);
        }
    }
    Vector3 ClosestPointOnMeshOBB(MeshFilter meshFilter, Vector3 worldPoint) {
        // First, we transform the point into the local coordinate space of the mesh.
        var localPoint = meshFilter.transform.InverseTransformPoint(worldPoint);

        // Next, we compare it against the mesh's axis-aligned bounds in its local space.
        var localClosest = meshFilter.sharedMesh.bounds.ClosestPoint(localPoint);

        // Finally, we transform the local point back into world space.
        return meshFilter.transform.TransformPoint(localClosest);
    }
}
