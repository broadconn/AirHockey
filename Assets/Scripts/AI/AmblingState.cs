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
            curCenter = Ctx.AiMallet.Rb.position;
            timeEnteredState = Ctx.Time;
        }

        public override AIMalletState UpdateState() {
            // if the puck is heading towards us, change to interception mode. 
            var puckIsMoving = Ctx.Puck.Rb.velocity.magnitude > 0;
            if (puckIsMoving && (Ctx.PuckMovingTowardsAI || (Ctx.PuckOnOurSide && Ctx.PuckMovingAwayTooSlow)))
                return AIMalletState.Intercepting;

            return AIMalletState.Ambling;
        }

        public override Vector3 UpdatePosition() {
            amble = UpdateAmble();
            ambleCenter = GetAmbleCenter();
            //var perc = Mathf.Clamp01((Ctx.Time - timeEnteredState) / timeReachAmbleCenter);
            curCenter = ambleCenter;// Vector3.Lerp(curCenter, ambleCenter, perc);

            return curCenter + amble;
        }

        /// <summary>
        /// The swaying offset that will be applied to a center point
        /// </summary>
        /// <returns></returns>
        private Vector3 UpdateAmble() {
            var t = Time.time;
            var xMax = Mathf.Lerp(GameController.Instance.MalletAIAmbleX.x, GameController.Instance.MalletAIAmbleX.y, Ctx.Riskyness.Value);
            var yMax = GameController.Instance.MalletAIAmbleY;
            var x = Mathf.Sin(t / ambleOscillateTime ) * xMax;
            var y = Mathf.Sin(t / ambleOscillateTime * 2) * yMax; // x2 to create a figure-eight pattern
            return new Vector3(x, 0, y);
        }

        private Vector3 GetAmbleCenter() {
            const float offset = 0.5f; // distance out from the goal / center-line to amble
            var unconfidentEnd = Ctx.AIGoalPos + (Ctx.ArenaCenterPos - Ctx.AIGoalPos).normalized * offset;
            var confidentEnd = Ctx.ArenaCenterPos + (Ctx.AIGoalPos - Ctx.ArenaCenterPos).normalized * offset;
            var ambleLocation = Vector3.Lerp(unconfidentEnd, confidentEnd, Ctx.Riskyness.Value);

            // this might be better as a different state (position behind puck)
            if (ambleLocation.z < Ctx.Puck.Rb.position.z) {
                ambleLocation = new Vector3(ambleLocation.x, ambleLocation.y, Ctx.Puck.Rb.position.z + Ctx.AiMallet.Radius);
            }

            return ambleLocation;
        }
    }
}
