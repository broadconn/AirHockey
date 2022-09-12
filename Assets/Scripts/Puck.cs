using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puck : MonoBehaviour
{
    [SerializeField] float maxSpeed = 20;
    [SerializeField] float minSpeed = 2;
    [SerializeField] TrailRenderer trail;
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
        if (rb.velocity.magnitude > maxSpeed) {
            var s = rb.velocity.normalized * maxSpeed;
            rb.velocity = s;
        }

        // min speed
        if (rb.velocity.magnitude > 0 // only enforce if the puck be moving already
            && rb.velocity.magnitude < minSpeed) {
            var s = rb.velocity.normalized * minSpeed;
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
