using Assets.Scripts.AI;
using System;
using UnityEngine;

public class AIMallet : MonoBehaviour
{
    [SerializeField] MeshFilter malletArea;
    [SerializeField] Puck puck; 

    public Rigidbody Rb { get => rb; }
    Rigidbody rb;

    Vector3 desiredPosOnMesh;
    readonly Vector3 vectorTowardsMyZone = new (0, 0, 1);

    // ambling settings
    Vector3 amble;
    float timeNextAmbleUpdate = 0;
    float ambleSeedX, ambleSeedY;
    float minAmbleUpdateMS = 1, maxAmbleUpdateMS = 10;
    float maxAmbleRange = 2f;
    bool isAmbling = false;
    Vector3 ambleCenter;

    IAIState aiState;
    AIMalletState curState = AIMalletState.Paused;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        desiredPosOnMesh = transform.position;

        ambleSeedX = UnityEngine.Random.Range(0, 1000);
        ambleSeedY = UnityEngine.Random.Range(0, 1000);
    }

    private void Update() {
        UpdateAmble();
        UpdateTgtPos();
    }

    void UpdateAmble() {
        if (Time.time > timeNextAmbleUpdate) {
            var ambleAmount = UnityEngine.Random.Range(0f, maxAmbleRange);
            amble = new Vector3(Mathf.PerlinNoise(ambleSeedX, Time.time) - 0.5f, 0, Mathf.PerlinNoise(Time.time, ambleSeedY) - 0.5f).normalized * ambleAmount;
            timeNextAmbleUpdate = Time.time + UnityEngine.Random.Range(minAmbleUpdateMS, maxAmbleUpdateMS);
        }
    }

    private void FixedUpdate() {
        var vectorToDesiredPos = desiredPosOnMesh - transform.position;
        var moveSpeed = GameController.Instance.MalletAIMaxSpeed;
        rb.MovePosition(transform.position + moveSpeed * Time.deltaTime * vectorToDesiredPos);
    }

    // TODO NEXT:
    // - strike puck if close, short delay before can react again
    // - short delay to react to the players puck hit
    // - keep pursuing the puck if the y velocity to the player is slow and its still on the red side
    // - ambient mode:
    //      - right after hitting the puck, if the puck isn't in danger of being hit by the player
    //      - 
    // - guard mode
    //      - if the player is close to hitting the puck
    //      - pretend to predict where the puck needs to be to intercept the puck
    //          - should always be 
    // - if the player looks like they will hit the puck close to the centerline, guard further back. If they hit it from their 
    void UpdateTgtPos() {
        // do AI stuff
        var puckPosition = puck.GetComponent<Rigidbody>().position;
        var puckVelocity = puck.GetComponent<Rigidbody>().velocity;
        var puckDirectionAngle = Vector3.Angle(puckVelocity, vectorTowardsMyZone);
        var distToPuck = Vector3.Distance(rb.position, puckPosition);
        var futurePuckPos = puckPosition + puckVelocity.normalized * GameController.Instance.MalletAIPuckProjectionDist;

        Vector3 desiredPos;

        // if puck is not travelling towards our zone, relax, amble around.
        //  depending on how high the difficulty is, easy -> roam randomly, hard -> amble around the goal
        var shouldAmble = puckDirectionAngle > 90;
        if (shouldAmble) {
            if (curState != AIMalletState.Ambling) {
                curState = AIMalletState.Ambling;
                ambleCenter = rb.position;
            }
            desiredPos = ambleCenter + amble;
        }
        //if puck is travelling towards our zone, move to an interception point
        else { 
            desiredPos = futurePuckPos;
        }

        //if mallet is close enough to the puck, strike at it
        if (distToPuck < GameController.Instance.MalletAIStrikeDistance) 
            desiredPos = puckPosition;

        desiredPosOnMesh = ClosestPointOnMesh(malletArea, desiredPos);
    }

    Vector3 ClosestPointOnMesh(MeshFilter meshFilter, Vector3 worldPoint) {
        var localPoint = meshFilter.transform.InverseTransformPoint(worldPoint); 
        var localClosest = meshFilter.sharedMesh.bounds.ClosestPoint(localPoint);
        return meshFilter.transform.TransformPoint(localClosest); 
    }
}

public enum AIMalletState {
    Paused,
    Ambling,
    Intercepting,
    Striking
}
