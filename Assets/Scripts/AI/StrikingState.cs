using UnityEngine;

namespace Assets.Scripts.AI {
    /// <summary>
    /// Strike at the puck, only change state once we have "recovered" from striking the puck.
    /// </summary>
    internal class StrikingState : AIState {
        float timeToRecoverFromHittingPuck = 0.5f;

        public StrikingState(AIContext context) : base(context) { }

        public override Vector3 UpdatePosition() {
            // head towards the puck with max speed.
            // be smart about the interception point.
            //      if the player is not in between the puck and the goal, strike straight for the goal (can randomize the chance of this)
            //      else try to bounce it off either wall (45 degrees?).

            var strikeForceKinda = 2f; //really just the distance behind the puck the mallet will try to move to
            var puckPosition = ctx.Puck.Rb.position;
            var puckVelocity = ctx.Puck.Rb.velocity;
            var strikePuckPos = puckPosition - puckVelocity.normalized * strikeForceKinda;
            return strikePuckPos;
        }

        public override AIMalletState UpdateState() {
            // if the puck is heading away, lose interest
            if (ctx.PuckDirectionAngle < 90)
                return AIMalletState.Ambling;

            return AIMalletState.Striking;
        }
    }
}
