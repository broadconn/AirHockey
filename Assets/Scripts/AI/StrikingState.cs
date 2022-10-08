using System;
using UnityEngine;

namespace Assets.Scripts.AI {
    /// <summary>
    /// Strike at the puck, only change state once we have "recovered" from striking the puck.
    /// </summary>
    internal class StrikingState : AIState {
        const float timeToRecoverFromStrikingPuck = 0.2f;
        const float strikeForceKinda = 5f; //really just the distance behind the puck the mallet will try to move to

        Vector3 lastStrikeTgtPos; // the position to strike towards
        Vector3 lastStrikeReturnPos; // the position to return to after striking the puck

        public StrikingState(AIContext context) : base(context) { }

        public override AIMalletState UpdateState() {
            // if the puck has recently struck, it is stunlocked for a bit.
            if (OnStrikeCooldown()) return AIMalletState.Striking;

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
             
            if (!OnStrikeCooldown()) 
                Strike(); 

            var strikePercToReturnPos = TimeSinceLastStruck() / timeToRecoverFromStrikingPuck;
            return Vector3.Lerp(lastStrikeReturnPos, lastStrikeTgtPos, GameController.Instance.MalletAIStrikeCurve.Evaluate(strikePercToReturnPos));
        }

        void Strike() {
            ctx.TimeLastStruckPuck = ctx.Time;
            var puckPosition = ctx.Puck.Rb.position;
            var puckVelocity = ctx.Puck.Rb.velocity;
            lastStrikeTgtPos = puckPosition - puckVelocity.normalized * strikeForceKinda;
            lastStrikeReturnPos = ctx.AiMallet.Rb.position;
        }


        /// <summary>
        /// Adjust our position a little so we're 
        /// a) not always just hitting the puck straight back the way it came
        /// b) trying to aim at the opponent goal
        /// </summary>
        /// <param name="pathPoint"></param>
        /// <returns></returns>
        Vector3 GetStrikePoint() {
            var dirToGoal = (ctx.AIGoalPos - ctx.AiMallet.Rb.position).normalized;
            var offsetAmount = ctx.AiMallet.Radius;
            return ctx.Puck.Rb.position + dirToGoal * offsetAmount;
        }

        bool OnStrikeCooldown() {
            return TimeSinceLastStruck() < timeToRecoverFromStrikingPuck;
        }

        float TimeSinceLastStruck() {
            return ctx.Time - ctx.TimeLastStruckPuck;
        }
    }
}
