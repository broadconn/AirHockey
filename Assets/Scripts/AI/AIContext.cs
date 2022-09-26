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
        public float Time { get; set; }
        public float TimeInState { get; set; }
        public bool StateChangedThisUpdate { get; set; }
        public Vector3 VectorTowardsPlayerZone { get => new(0, 0, 1); }
        public float TimeLastStruckPuck { get; set; } = -999;

        public float AiDistFromPuck { get => aiDistFromPuck; }
        float aiDistFromPuck = 0;
        public float PuckDirectionAngle { get => puckDirectionAngle; }
        float puckDirectionAngle = 0;

        public AIContext(AIMallet aiMallet, Puck puck, PuckFuturePath puckFuture, PlayerMallet player) {
            AiMallet = aiMallet;
            Puck = puck;
            PuckFuturePath = puckFuture;
            Player = player;
        }

        // might be better to lazy-load these per-frame
        public void UpdateCommonCalcs() {
            aiDistFromPuck = Vector3.Distance(AiMallet.Rb.position, Puck.Rb.position);

            var puckVelocity = Puck.Rb.velocity;
            puckDirectionAngle = Vector3.Angle(puckVelocity, VectorTowardsPlayerZone);
        }
    }
}
