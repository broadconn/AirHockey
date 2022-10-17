using UnityEngine;

namespace Assets.Scripts.AI {
    /// <summary>
    /// Puck is moving away, towards player or player hasn't hit the puck yet. 
    /// Do what real people do, hover randomly near the goal, or closer to the centerline if the player is closer to their goal.
    /// </summary>
    internal class AmblingState : AIState {
        Vector3 amble; 
        Vector3 ambleCenter; // the target center point
        Vector3 curCenter; // smoothed center point

        const float timeReachAmbleCenter = 3;
        const float ambleOscillateTime = 0.5f;
        float timeEnteredState = 0;

        public AmblingState(AIContext context) : base(context) { }

        public override void OnEnterState() {
            curCenter = ctx.AiMallet.Rb.position;
            timeEnteredState = ctx.Time;
        }

        public override AIMalletState UpdateState() {
            // if the puck is heading towards us, change to interception mode. 
            var puckIsMoving = ctx.Puck.Rb.velocity.magnitude > 0;
            if (puckIsMoving && (ctx.PuckMovingTowardsAI || (ctx.PuckOnOurSide && ctx.PuckMovingAwayTooSlow)))
                return AIMalletState.Intercepting;

            return AIMalletState.Ambling;
        }

        public override Vector3 UpdatePosition() {
            amble = UpdateAmble();
            ambleCenter = GetAmbleCenter();
            var perc = Mathf.Clamp01((ctx.Time - timeEnteredState) / timeReachAmbleCenter);
            curCenter = ambleCenter;// Vector3.Lerp(curCenter, ambleCenter, perc);

            return curCenter + amble;
        }

        /// <summary>
        /// The swaying offset that will be applied to a center point
        /// </summary>
        /// <returns></returns>
        Vector3 UpdateAmble() {
            var t = Time.time;
            var xMax = Mathf.Lerp(GameController.Instance.MalletAIAmbleX.x, GameController.Instance.MalletAIAmbleX.y, ctx.Riskyness.Value);
            var yMax = GameController.Instance.MalletAIAmbleY;
            var x = Mathf.Sin(t / ambleOscillateTime ) * xMax;
            var y = Mathf.Sin(t / ambleOscillateTime * 2) * yMax; // x2 to create a figure-eight pattern
            return new Vector3(x, 0, y);
        }

        Vector3 GetAmbleCenter() {
            var offset = 0.5f; // distance out from the goal / centerline to amble
            var unconfidentEnd = ctx.AIGoalPos + (ctx.ArenaCenterPos - ctx.AIGoalPos).normalized * offset;
            var confidentEnd = ctx.ArenaCenterPos + (ctx.AIGoalPos - ctx.ArenaCenterPos).normalized * offset;
            var ambleLocation = Vector3.Lerp(unconfidentEnd, confidentEnd, ctx.Riskyness.Value);

            // this might be better as a different state (position behind puck)
            if (ambleLocation.z < ctx.Puck.Rb.position.z) {
                ambleLocation = new Vector3(ambleLocation.x, ambleLocation.y, ctx.Puck.Rb.position.z + ctx.AiMallet.Radius);
            }

            return ambleLocation;
        }
    }
}
