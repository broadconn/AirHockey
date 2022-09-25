using UnityEngine;

namespace Assets.Scripts.AI { 
    internal class AIContext {
        public AIMallet AiMallet { get; }
        public Puck Puck { get; }
        public PlayerMallet Player { get; }
        public float Time { get; set; }
        public float TimeInState { get; set; }
        public bool ChangedStateThisUpdate { get; set; }
        public Vector3 VectorTowardsPlayerZone { get => new(0, 0, 1); }

        public float AiDistFromPuck { get => aiDistFromPuck; }
        float aiDistFromPuck = 0;
        public float PuckDirectionAngle { get => puckDirectionAngle; }
        float puckDirectionAngle = 0;

        public AIContext(AIMallet aiMallet, Puck puck, PlayerMallet player) {
            AiMallet = aiMallet;
            Puck = puck;
            Player = player;
        }

        public void UpdateCommonCalcs() {
            aiDistFromPuck = Vector3.Distance(AiMallet.Rb.position, Puck.Rb.position);

            var puckVelocity = Puck.Rb.velocity;
            puckDirectionAngle = Vector3.Angle(puckVelocity, VectorTowardsPlayerZone);
        }
    }
}
