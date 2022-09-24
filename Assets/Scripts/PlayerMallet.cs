using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMallet : MonoBehaviour
{
    [SerializeField] new Camera camera;
    [SerializeField] MeshFilter malletArea;

    public Rigidbody Rb { get => rb; } 
    Rigidbody rb;

    Vector3 tgtPos;
    const int mouseAreaColliderLayer = 6;

    // Start is called before the first frame update
    void Start()
    {
        //Cursor.visible = false; 
        rb = GetComponent<Rigidbody>();
        tgtPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() {
        UpdateTgtPos();

        var vecToTgtPos = tgtPos - transform.position;
        rb.MovePosition(transform.position + GameController.Instance.MalletPlayerMaxSpeed * Time.deltaTime * vecToTgtPos);
    }

    void UpdateTgtPos() {
        int layerMask = 1 << mouseAreaColliderLayer; // only cast rays against the P1 mouse area collider

        // try to go to the world position the mouse points to
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
