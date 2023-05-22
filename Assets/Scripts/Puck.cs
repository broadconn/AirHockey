using Mono.Cecil;
using UnityEngine;

public class Puck : MonoBehaviour {
    [SerializeField] TrailRenderer trail;
    [SerializeField] Transform player;
    [SerializeField] Transform ai;
    [SerializeField] Transform playerPuckSpawnPos;
    [SerializeField] Transform aiPuckSpawnPos;
    public Vector3 AiPuckSpawnPos { get => aiPuckSpawnPos.position; }


    public Rigidbody Rb { get => rb; }

    Rigidbody rb;

    new SphereCollider collider;
    Vector3 lastCollisionPos; 
    float timeLastHitWall = 0;
    int playerServing = 0;
     
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        //collider.enabled = rb.velocity.magnitude > 0 || PlayerServingInZPosBehindPuck();
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

    bool PlayerServingInZPosBehindPuck() {
        if (playerServing == 1) {
            var safeZ = playerPuckSpawnPos.position.z;
            return player.position.z < safeZ;
        }
        else {
            var safeZ = aiPuckSpawnPos.position.z;
            return ai.position.z > safeZ;
        }
    }

    private void OnCollisionEnter(Collision collision) {
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

    public void ResetForNewRound(bool p) {
        transform.position = playerServing == 1 ? playerPuckSpawnPos.position : aiPuckSpawnPos.position;
        //collider.enabled = false;
        trail.Clear();
        ZeroVelocity();
    }

    private void ZeroVelocity() {
        rb = rb != null ? rb : GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
    }
}
