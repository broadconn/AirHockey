using UnityEngine;

namespace Assets.Scripts.AI {
    /// <summary>
    /// Puck is moving away, towards player or player hasn't hit the puck yet. 
    /// Do what real people do, hover randomly near the goal, or closer to the centerline if the player is closer to their goal.
    /// </summary>
    internal class AmblingState : AIState { //AnticipationState?
        // TODO: CHANGE THIS TO MORE SWAYING MOTIONS (sin() on XZ with wide X, thin Z), LIKE A NORMAL PERSON
        //          maybe choose either the sway or a random motion.

        // random ambling settings
        Vector3 amble;
        float timeNextAmbleUpdate = 0;
        float ambleSeedX, ambleSeedY;
        const float minAmbleUpdateS = 0.1f, maxAmbleUpdateS = 3f;
        const float maxAmbleRange = 1f;
        Vector3 ambleCenter;

        public AmblingState(AIContext context) : base(context) {
            ambleSeedX = Random.Range(0, 1000);
            ambleSeedY = Random.Range(0, 1000);
        }

        public override Vector3 UpdatePosition() {
            return ctx.AiMallet.Rb.position;

            if (ctx.Time > timeNextAmbleUpdate) {
                var ambleAmount = Random.Range(0f, maxAmbleRange);
                amble = new Vector3(Mathf.PerlinNoise(ambleSeedX, ctx.Time) - 0.5f, 0, Mathf.PerlinNoise(ctx.Time, ambleSeedY) - 0.5f).normalized * ambleAmount;
                timeNextAmbleUpdate = ctx.Time + Random.Range(minAmbleUpdateS, maxAmbleUpdateS);
            }

            if (ctx.ChangedStateThisUpdate) {
                ambleCenter = ctx.AiMallet.Rb.position;
            }

            return ambleCenter + amble;
        }

        public override AIMalletState UpdateState() {
            // TODO: change this to more accurately predict when the ai should start "being serious"

            // if the puck is heading towards us, change to interception mode
            var puckVelocity = ctx.Puck.Rb.velocity;
            if (puckVelocity.magnitude > 0 && ctx.PuckDirectionAngle < 90)
                return AIMalletState.Intercepting;

            return AIMalletState.Ambling;
        }
    }
}
