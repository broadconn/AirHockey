using Assets.Scripts.AI;
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
        public float TimeInRound { get; set; }
        public bool StateChangedThisUpdate { get; set; }
        public Vector3 VectorTowardsPlayerZone { get => new(0, 0, 1); }
        public float TimeLastStruckPuck { get; set; } = -999;
        public bool PlayerScoredLast { get; set; } = false;
        public Riskyness Riskyness { get; set; }

        // helper properties
        public float AiDistFromPuck { get => Vector3.Distance(AiMallet.Rb.position, Puck.Rb.position); }
        public bool PuckMovingTowardsAI { get => Puck.Rb.velocity.z > 0; }
        public bool PuckMovingAway { get => Puck.Rb.velocity.z < 0; }
        public bool PuckMovingAwayTooSlow { get => Puck.Rb.velocity.z > -GameController.Instance.MalletAiMaxSpeedToStillChasePuck; }
        public bool PuckOnOurSide { get => Puck.Rb.position.z > ArenaCenterPos.z; }
        public bool WithinStrikingDistance { get => AiDistFromPuck < GameController.Instance.MalletAiStrikeDistance; }
        public bool AIBehindPuck { get => AiMallet.Rb.position.z > Puck.Rb.position.z; }

        public AIContext(AIMallet aiMallet, Puck puck, PuckFuturePath puckFuture, PlayerMallet player, Vector3 arenaCenterPos, Vector3 aiGoalPos) {
            AiMallet = aiMallet;
            Puck = puck;
            PuckFuturePath = puckFuture;
            Player = player;
            ArenaCenterPos = arenaCenterPos;
            AIGoalPos = aiGoalPos;
            Riskyness = new(this);
        }

        public void Update() {
            Riskyness.Update();
        }
    }
}

/// <summary>
/// Affects how guardedly the AI plays.
/// </summary>
internal class Riskyness {
    readonly AIContext ctx;

    public float Value { get => confidence + impatience;  }

    // dynamic value based on how the game is going for the AI
    float confidence = 0.5f;
    const float deltaOnPlayerGoalScore = 0.2f;
    const float deltaOnMeGoalScore = 0.4f;

    // grows from 0 every round
    float impatience = 0;
    const float impatienceSpeed = 0.01f; // how quickly impatience grows during a round

    // riskyness determines how far away from the goal it passively hangs around.
    // things that affect riskyness:
    //  - score (grow confidence), get scored against (deflate confidence) 
    //  - grow impatient during a round

    // examples how to use: affect the speed and smoothness of the amble
    // if low riskyness, stay by the goal and do smooth, consistent movements (like it's acting seriously >:| )
    // if big riskyness, stay around the centerline and move sporadically (like it's gloating >:D )
    internal Riskyness(AIContext ctx) { this.ctx = ctx; }

    public void Update() {
        impatience = ctx.TimeInRound * impatienceSpeed;
    }

    public void OnGoalScored(bool playerScored) {
        confidence += playerScored ? -deltaOnPlayerGoalScore : deltaOnMeGoalScore;
        confidence = Mathf.Clamp01(confidence);
    }

    public void DebugForceValue(float confidence, float impatience = 0) {
        this.confidence = confidence;
        this.impatience = impatience;
    }
}
