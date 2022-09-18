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
        if (Physics.Raycast(ray, out RaycastHit hit, 1000, layerMask)) {
            Vector3 hitWorldPos = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            tgtPos = ClosestPointOnMesh(malletArea, hitWorldPos);
        }
    }

    Vector3 ClosestPointOnMesh(MeshFilter meshFilter, Vector3 worldPoint) {
        var localPoint = meshFilter.transform.InverseTransformPoint(worldPoint); 
        var localClosest = meshFilter.sharedMesh.bounds.ClosestPoint(localPoint);
        return meshFilter.transform.TransformPoint(localClosest); 
    }
}
