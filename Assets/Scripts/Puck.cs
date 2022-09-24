using UnityEngine;

public class Puck : MonoBehaviour {
    [SerializeField] TrailRenderer trail;

    public Rigidbody Rb { get => rb; }
    Rigidbody rb; 

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    { 
    }

    private void FixedUpdate() { 
        // max speed
        if (rb.velocity.magnitude > GameController.Instance.PuckMaxSpeed) {
            var s = rb.velocity.normalized * GameController.Instance.PuckMaxSpeed;
            rb.velocity = s;
        }

        // min speed
        if (rb.velocity.magnitude > 0 // only enforce if the puck be moving already
            && rb.velocity.magnitude < GameController.Instance.PuckMinSpeed) {
            var s = rb.velocity.normalized * GameController.Instance.PuckMinSpeed;
            rb.velocity = s;
        }
    }

    public void ResetToPosition(Vector3 pos) {
        transform.position = pos;
        trail.Clear();
        ZeroVelocity();
    }

    private void ZeroVelocity() {
        rb = rb != null ? rb : GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
    }
}
