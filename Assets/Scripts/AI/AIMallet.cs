using Assets;
using Assets.Scripts.AI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AIMallet : MonoBehaviour {
    [Header("AI Context")]
    [SerializeField] MeshFilter malletArea;
    [SerializeField] Puck puck;
    [SerializeField] PuckFuturePath puckFuturePath;
    [SerializeField] PlayerMallet player;
    [SerializeField] Transform arenaCenter;
    [SerializeField] Transform myGoal;

    [Header("Debug")]
    [SerializeField] TextMeshProUGUI debugStateText;
    [SerializeField] TextMeshProUGUI debugConfidence;

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
        UpdatePosition();
    }

    void HandleDebug() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { context.Confidence.DebugForceValue(0.1f); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { context.Confidence.DebugForceValue(0.2f); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { context.Confidence.DebugForceValue(0.3f); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { context.Confidence.DebugForceValue(0.4f); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { context.Confidence.DebugForceValue(0.5f); }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { context.Confidence.DebugForceValue(0.6f); }
        if (Input.GetKeyDown(KeyCode.Alpha7)) { context.Confidence.DebugForceValue(0.7f); }
        if (Input.GetKeyDown(KeyCode.Alpha8)) { context.Confidence.DebugForceValue(0.8f); }
        if (Input.GetKeyDown(KeyCode.Alpha9)) { context.Confidence.DebugForceValue(0.9f); }
    }

    void UpdateContext() {
        context.Time = Time.time;
        context.TimeInState += Time.deltaTime;
        context.StateChangedThisUpdate = false;
        context.UpdateCommonCalcs();
    }

    void UpdateState() {
        var newState = stateToProcessor[curState].UpdateState();
        if (curState != newState) {
            context.StateChangedThisUpdate = true; 
            context.TimeInState = 0; 
        }
        curState = newState;

        // debug
        debugStateText.text = curState.ToString();
        debugConfidence.text = context.Confidence.Value.ToString();
    }

    void UpdatePosition() {
        var desiredPos = stateToProcessor[curState].UpdatePosition();

        // limit position to the mallet area mesh
        desiredPosOnMesh = ClosestPointOnMesh(malletArea, desiredPos);
    }

    public void OnGoalScored(object e, GoalScoredEventArgs playerScored) {
        context.PlayerScoredLast = playerScored.PlayerNum == 1;
        context.Confidence.OnGoalScored(context.PlayerScoredLast);
    }

    public void ResetForNewRound() {
        curState = AIMalletState.Ambling;        
    }

    private void FixedUpdate() {
        var vectorToDesiredPos = desiredPosOnMesh - transform.position;
        var moveSpeed = GameController.Instance.MalletAIMaxSpeed;
        var move = moveSpeed * Time.deltaTime * vectorToDesiredPos.normalized;
        if (move.magnitude > vectorToDesiredPos.magnitude)
            move = vectorToDesiredPos;
        rb.MovePosition(transform.position + move);
    }

    Vector3 ClosestPointOnMesh(MeshFilter meshFilter, Vector3 worldPoint) {
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
