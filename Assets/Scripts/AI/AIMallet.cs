using Assets;
using Assets.Scripts.AI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIMallet : MonoBehaviour {
    [Header("AI Context")]
    [SerializeField] MeshFilter malletArea;
    [SerializeField] Puck puck;
    [SerializeField] PuckFuturePath puckFuturePath;
    [SerializeField] PlayerMallet player;
    [SerializeField] Transform arenaCenter;
    [SerializeField] Transform myGoal;

    [Header("Debug")]
    [SerializeField] Text debugStateText;
    [SerializeField] Text debugConfidence;

    AIContext context;

    public Rigidbody Rb { get => rb; }
    Rigidbody rb;

    public float Radius { get => radius; }
    private float radius;

    Vector3 desiredPosOnMesh;
     
    AIMalletState curState;

    Dictionary<AIMalletState, AIState> stateToProcessor;


    private void Awake() {
        rb = GetComponent<Rigidbody>();
        radius = GetComponent<SphereCollider>().radius * transform.localScale.z;

        context = new AIContext(this, puck, puckFuturePath, player, arenaCenter.position, myGoal.position);
        stateToProcessor = new() {
            { AIMalletState.Paused, new PausedState(context) },
            { AIMalletState.Striking, new StrikingState(context) },
            { AIMalletState.Intercepting, new InterceptingState(context) },
            { AIMalletState.Ambling, new AmblingState(context) }
        };

        curState = AIMalletState.Ambling;
    }

    private void Start() {
        GameController.Instance.GoalScoredEvent += OnGoalScored;
    }

    private void Update() {
        HandleDebug();

        UpdateContext();

        UpdateState();
        UpdateTgtPosition();
    }

    void HandleDebug() {
        if (Input.GetKeyDown(KeyCode.Alpha0)) { context.Riskyness.DebugForceValue(0.0f); }
        if (Input.GetKeyDown(KeyCode.Alpha1)) { context.Riskyness.DebugForceValue(0.1f); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { context.Riskyness.DebugForceValue(0.2f); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { context.Riskyness.DebugForceValue(0.3f); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { context.Riskyness.DebugForceValue(0.4f); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { context.Riskyness.DebugForceValue(0.5f); }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { context.Riskyness.DebugForceValue(0.6f); }
        if (Input.GetKeyDown(KeyCode.Alpha7)) { context.Riskyness.DebugForceValue(0.7f); }
        if (Input.GetKeyDown(KeyCode.Alpha8)) { context.Riskyness.DebugForceValue(0.8f); }
        if (Input.GetKeyDown(KeyCode.Alpha9)) { context.Riskyness.DebugForceValue(0.9f); }
    }

    void UpdateContext() {
        context.Time = Time.time;
        context.TimeInState += Time.deltaTime;
        context.TimeInRound += Time.deltaTime;
        context.StateChangedThisUpdate = false;
        context.Update();
    }

    void UpdateState() {
        var newState = stateToProcessor[curState].UpdateState();
        if (curState != newState) {
            context.StateChangedThisUpdate = true; 
            context.TimeInState = 0; 
        }
        curState = newState;
         
        if (context.StateChangedThisUpdate)
            stateToProcessor[curState].OnEnterState();

        // debug
        debugStateText.text = curState.ToString();
        debugConfidence.text = context.Riskyness.Value.ToString("F3");
    }

    void UpdateTgtPosition() {
        var desiredPos = stateToProcessor[curState].UpdatePosition();
        var malletPos = transform.position;
        desiredPosOnMesh = GetPointOnMeshAlongLineToPoint(malletArea, malletPos, desiredPos);
    }

    public void OnGoalScored(object e, GoalScoredEventArgs playerScored) {
        context.TimeInRound = 0;

        context.PlayerScoredLast = playerScored.PlayerNum == 1;
        context.Riskyness.OnGoalScored(context.PlayerScoredLast);
    }

    public void ResetForNewRound() {
        curState = AIMalletState.Ambling;        
    }

    private void FixedUpdate() {
        DoMove();
    }

    void DoMove() { 
        var vectorToDesiredPos = desiredPosOnMesh - transform.position;
        var moveSpeed = GameController.Instance.MalletAIMaxSpeed;
        var frameMovement = moveSpeed * Time.deltaTime * vectorToDesiredPos.normalized;
        if (frameMovement.magnitude > vectorToDesiredPos.magnitude)
            frameMovement = vectorToDesiredPos;
        rb.MovePosition(transform.position + frameMovement);
    }

    /// <summary>
    /// Limits the position to the given mesh, along the line between the mallet and the world position. 
    /// See the test scene to highlight why refinements are required, but because the mesh is square, the closest point on the mesh won't be on the line between the mallet and the point,
    /// unless the world point is very close to the edge of the mesh. So if we sample again along the desired line at a distance that the first sample reached, it should be pretty close to the edge.
    /// Do this more times to get more accurate.
    /// </summary>
    /// <param name="meshFilter"></param>
    /// <param name="malletPos"></param>
    /// <param name="worldPoint"></param>
    /// <param name="numRefinements"></param>
    /// <returns></returns>
    public static Vector3 GetPointOnMeshAlongLineToPoint(MeshFilter meshFilter, Vector3 malletPos, Vector3 worldPoint, int numRefinements = 3) {
        var meshPoint = ClosestPointOnMesh(meshFilter, worldPoint);
        for (int i = 0; i < numRefinements; i++) {
            var malletToMeshPointVector = meshPoint - malletPos;
            var distToMeshPoint = malletToMeshPointVector.magnitude;
            var malletToWorldPointVector = worldPoint - malletPos;
            var closerSamplePoint = malletPos + malletToWorldPointVector.normalized * distToMeshPoint;
            meshPoint = ClosestPointOnMesh(meshFilter, closerSamplePoint);
        }
        return meshPoint;
    }

    public static Vector3 ClosestPointOnMesh(MeshFilter meshFilter, Vector3 worldPoint) {
        var localPoint = meshFilter.transform.InverseTransformPoint(worldPoint); 
        var localClosest = meshFilter.sharedMesh.bounds.ClosestPoint(localPoint);
        return meshFilter.transform.TransformPoint(localClosest); 
    }

    public Vector3 GetDestination() {
        return desiredPosOnMesh;
    }
}

public enum AIMalletState {
    Paused,
    Ambling,
    Intercepting,
    Striking
}
