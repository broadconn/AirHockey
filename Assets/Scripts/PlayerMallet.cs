using Assets.Scripts.Utility;
using UnityEngine;

public class PlayerMallet : MonoBehaviour {
    [SerializeField] private new Camera camera;
    [SerializeField] private MeshFilter malletArea;

    public Rigidbody Rb => rb;
    Rigidbody rb;

    Vector3 tgtPos;
    const int mouseAreaColliderLayer = 6;

    // Start is called before the first frame update
    void Start() {
        //Cursor.visible = false; 
        rb = GetComponent<Rigidbody>();
        tgtPos = transform.position;
    }

    private void FixedUpdate() {
        UpdateTgtPos();

        var position = transform.position;
        var vecToTgtPos = tgtPos - position;
        rb.MovePosition(position + GameController.Instance.MalletPlayerMaxSpeed * Time.deltaTime * vecToTgtPos);
    }

    void UpdateTgtPos() {
        const int layerMask = 1 << mouseAreaColliderLayer; // only cast rays against the P1 mouse area collider

        // try to go to the world position the mouse points to
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 1000, layerMask)) {
            var hitWorldPos = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            tgtPos = Utilities.GetClosestPointOnMeshAlongLineFromPoint(malletArea, hitWorldPos, camera.transform.position);
        }
    }
}
