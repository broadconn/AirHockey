using System;
using UnityEngine;

namespace Assets.Scripts.AI {
    /// <summary>
    /// Strike at the puck, only change state once we have "recovered" from striking the puck.
    /// </summary>
    internal class StrikingState : AIState {
        const float timeToRecoverFromStrikingPuck = 0.5f;
        const float strikeForceKinda = 2f; //really just the distance behind the puck the mallet will try to move to

        Vector3 lastStrikeTgtPos; // the position to strike towards
        Vector3 lastStrikeReturnPos; // the position to return to after striking the puck

        public StrikingState(AIContext context) : base(context) { }

        public override AIMalletState UpdateState() {
            // if the puck is heading away, lose interest
            var loseInterest = ctx.PuckDirectionAngle > 90;
            if (loseInterest)
                return AIMalletState.Ambling;

            return AIMalletState.Striking;
        }

        public override Vector3 UpdatePosition() {
            // head towards a point past the puck with max speed, then return to the position we started.
            // be smart about the interception point.
            //      if the player is not in between the puck and the goal, strike straight for the goal (can randomize the chance of this)
            //      else try to bounce it off either wall (45 degrees?).

            var timeSinceLastStruck = ctx.Time - ctx.TimeLastStruckPuck;
            var canHitPuckAgain = timeSinceLastStruck > timeToRecoverFromStrikingPuck;

            if (canHitPuckAgain) {
                ctx.TimeLastStruckPuck = ctx.Time;
                var puckPosition = ctx.Puck.Rb.position;
                var puckVelocity = ctx.Puck.Rb.velocity;
                lastStrikeTgtPos = puckPosition - puckVelocity.normalized * strikeForceKinda;
                lastStrikeReturnPos = ctx.AiMallet.Rb.position;
            }

            var strikePercToReturnPos = timeSinceLastStruck / timeToRecoverFromStrikingPuck;
            return Vector3.Lerp(lastStrikeTgtPos, lastStrikeReturnPos, strikePercToReturnPos);
        }
    }
}
