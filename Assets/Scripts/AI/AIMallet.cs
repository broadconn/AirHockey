using Assets.Scripts.AI;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using static UnityEditor.Rendering.InspectorCurveEditor;

public class AIMallet : MonoBehaviour
{
    [SerializeField] MeshFilter malletArea;
    [SerializeField] Puck puck;
    [SerializeField] PlayerMallet player;

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
        context.ChangedStateThisUpdate = false;
        context.UpdateCommonCalcs();
    }

    void UpdateState() {
        var newState = stateToProcessor[curState].UpdateState();
        if (curState != newState) { 
            context.ChangedStateThisUpdate = true; 
            context.TimeInState = 0; 
        }
        curState = newState;
    }

    void UpdatePosition() {
        var desiredPos = stateToProcessor[curState].UpdatePosition();

        // limit position to the mallet area mesh
        desiredPosOnMesh = ClosestPointOnMesh(malletArea, desiredPos);
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
