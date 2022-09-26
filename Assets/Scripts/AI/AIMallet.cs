using Assets.Scripts.AI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AIMallet : MonoBehaviour
{
    [SerializeField] MeshFilter malletArea;
    [SerializeField] Puck puck;
    [SerializeField] PlayerMallet player;

    [Header("Debug")]
    [SerializeField] TextMeshProUGUI debugStateText;

    AIContext context;

    public Rigidbody Rb { get => rb; }
    Rigidbody rb;

    Vector3 desiredPosOnMesh;
     
    AIMalletState curState;

    Dictionary<AIMalletState, AIState> stateToProcessor;


    private void Awake() {
        rb = GetComponent<Rigidbody>();

        context = new AIContext(this, puck, player);
        stateToProcessor = new() {
            { AIMalletState.Paused, new PausedState(context) },
            { AIMalletState.Striking, new StrikingState(context) },
            { AIMalletState.Intercepting, new InterceptingState(context) },
            { AIMalletState.Ambling, new AmblingState(context) }
        };

        curState = AIMalletState.Ambling;
    }

    private void Update() {
        UpdateContext();
        UpdateState();
        UpdatePosition();
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
            print($"State change!: {newState}");
            context.StateChangedThisUpdate = true; 
            context.TimeInState = 0; 
        }
        curState = newState;
        debugStateText.text = curState.ToString();
    }

    void UpdatePosition() {
        var desiredPos = stateToProcessor[curState].UpdatePosition();

        // limit position to the mallet area mesh
        desiredPosOnMesh = ClosestPointOnMesh(malletArea, desiredPos);
    }

    public void ResetForNewRound() {
        curState = AIMalletState.Ambling;        
    }

    private void FixedUpdate() {
        var vectorToDesiredPos = desiredPosOnMesh - transform.position;
        var moveSpeed = GameController.Instance.MalletAIMaxSpeed;
        rb.MovePosition(transform.position + moveSpeed * Time.deltaTime * vectorToDesiredPos);
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
