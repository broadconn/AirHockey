using Assets;
using Assets.Scripts.AI;
using Assets.Scripts.Utility;
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
    [SerializeField] Transform aiPuckServePos;
    public Vector3 ServePos { get => aiPuckServePos.position; }

    [Header("Debug")]
    [SerializeField] Text debugStateText;
    [SerializeField] Text debugConfidence;

    AIContext context;

    public Rigidbody Rb { get => rb; }
    Rigidbody rb;

    public float Radius { get => worldRadius; }
    private float worldRadius;

    Vector3 desiredPosOnMesh;
     
    AIMalletState curState;

    Dictionary<AIMalletState, AIState> stateToProcessor;


    private void Awake() {
        rb = GetComponent<Rigidbody>();
        worldRadius = GetComponent<SphereCollider>().radius * transform.localScale.z;

        context = new AIContext(this, puck, puckFuturePath, player, arenaCenter.position, myGoal.position);
        stateToProcessor = new() {
            { AIMalletState.Paused, new PausedState(context) },
            { AIMalletState.Striking, new StrikingState(context) },
            { AIMalletState.Intercepting, new InterceptingState(context) },
            { AIMalletState.Ambling, new AmblingState(context) },
            { AIMalletState.Serving, new ServingState(context) }
        };
    }

    private void Start() {
        GameController.Instance.GoalScoredEvent += OnGoalScored;
    }

    private void Update() {
        UpdateContext();
        UpdateState();

        ConstrainDesiredPosToMesh();

        if (GameController.Instance.debug)
            DebugStuff();
    }

    void DebugStuff() {
        debugStateText.text = curState.ToString();
        debugConfidence.text = context.Riskyness.Value.ToString("F3");

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
    }

    void ConstrainDesiredPosToMesh() {
        var desiredPos = stateToProcessor[curState].UpdatePosition();
        var malletPos = transform.position;
        desiredPosOnMesh = Utilities.GetClosestPointOnMeshAlongLineFromPoint(malletArea, malletPos, desiredPos);
    }

    public void OnGoalScored(object e, GoalScoredEventArgs scoringPlayerInfo) {
        context.TimeInRound = 0;
        context.PlayerScoredLast = scoringPlayerInfo.PlayerNum == 1;
        context.Riskyness.OnGoalScored(context.PlayerScoredLast);
    }

    public void ResetForNewRound(bool isServing) { 
        curState = isServing ? AIMalletState.Serving : AIMalletState.Ambling;
        stateToProcessor[curState].OnEnterState(); 
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

    public Vector3 GetDestination() {
        return desiredPosOnMesh;
    }
}

public enum AIMalletState {
    Paused,
    Ambling,
    Intercepting,
    Striking,
    Serving
}
