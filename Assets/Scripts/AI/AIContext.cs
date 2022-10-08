using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI { 
    /// <summary>
    /// Stuff the AI has access to. What it can "see" and use in it's decision-making.
    /// </summary>
    internal class AIContext {
        public AIMallet AiMallet { get; }
        public Puck Puck { get; }
        public PuckFuturePath PuckFuturePath { get; }
        public PlayerMallet Player { get; }
        public Vector3 ArenaCenterPos { get; }
        public Vector3 AIGoalPos { get; }
        public float Time { get; set; }
        public float TimeInState { get; set; }
        public bool StateChangedThisUpdate { get; set; }
        public Vector3 VectorTowardsPlayerZone { get => new(0, 0, 1); }
        public float TimeLastStruckPuck { get; set; } = -999;
        public bool PlayerScoredLast { get; set; } = false;
        public Confidence Confidence { get; set; }

        public float AiDistFromPuck { get => aiDistFromPuck; }
        float aiDistFromPuck = 0;
        public float PuckDirectionAngle { get => puckDirectionAngle; }
        float puckDirectionAngle = 0;

        public AIContext(AIMallet aiMallet, Puck puck, PuckFuturePath puckFuture, PlayerMallet player, Vector3 arenaCenterPos, Vector3 aiGoalPos) {
            AiMallet = aiMallet;
            Puck = puck;
            PuckFuturePath = puckFuture;
            Player = player;
            ArenaCenterPos = arenaCenterPos;
            AIGoalPos = aiGoalPos;
            Confidence = new();
        }

        // consider lazy-loading these as needed per-frame
        public void UpdateCommonCalcs() {
            aiDistFromPuck = Vector3.Distance(AiMallet.Rb.position, Puck.Rb.position);

            var puckVelocity = Puck.Rb.velocity;
            puckDirectionAngle = Vector3.Angle(puckVelocity, VectorTowardsPlayerZone);
        }
    }
}

/// <summary>
/// Affects how risky / protectively the AI plays.
/// </summary>
public class Confidence {
    float value = 0.5f;
    public float Value { get => value; set => this.value = Mathf.Clamp01(value); }

    const float deltaOnGoalScore = 0.2f;

    // confidence determines how far away from the goal it should amble.
    // things that affect cockiness: scored 2 in a row (grow confidence), got scored against (deflate confidence) 

    // confidence can also affect the speed and smoothness of the amble
    // if smol confidence, stay by the goal and do smooth, consistent movements (like it's acting seriously >:| )
    // if big confidence, stay around the centerline and move sporadically (like it's gloating >:D )
    public Confidence() { }

    public void OnGoalScored(bool playerScored) {
        Value += playerScored ? -deltaOnGoalScore : deltaOnGoalScore;
    }

    public void DebugForceValue(float val) {
        value = val;
    }
}
