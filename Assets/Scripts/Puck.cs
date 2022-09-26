using UnityEngine;

public class Puck : MonoBehaviour {
    [SerializeField] TrailRenderer trail;

    public Rigidbody Rb { get => rb; }
    Rigidbody rb;

    Vector3 lastCollisionPos; 
    float timeLastHitWall = 0;
     
    void Awake()
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

    private void OnCollisionEnter(Collision collision) {
        // sanity check dist / velocity = time
        if (collision.transform.CompareTag("Player")) {
            timeLastHitWall = Time.time;
            lastCollisionPos = rb.position;
        }
        if (collision.transform.CompareTag("Wall")) {
            var actualTime = Time.time - timeLastHitWall;
            var distSince = Vector3.Distance(lastCollisionPos, rb.position);
            timeLastHitWall = Time.time;
            lastCollisionPos = rb.position;
            var calcTime = distSince / rb.velocity.magnitude;
            var diff = calcTime - actualTime;
            //print($"bump: Vel:{rb.velocity.magnitude:F3} Dist:{distSince:F3} ActualTime:{actualTime:F3} CalculatedTime:{calcTime:F3} Diff:{diff:F3}");
        }
    }

    public void ResetForNewRound(Vector3 pos) {
        transform.position = pos;
        trail.Clear();
        ZeroVelocity();
    }

    private void ZeroVelocity() {
        rb = rb != null ? rb : GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
    }
}
