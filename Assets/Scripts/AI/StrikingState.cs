using System;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace Assets.Scripts.AI {
    /// <summary>
    /// Strike at the puck, only change state once we have "recovered" from striking the puck.
    /// </summary>
    internal class StrikingState : AIState {
        Vector3 strikeTgtPos; // the position to strike towards
        Vector3 strikeReturnPos; // the position to return to after striking the puck

        public StrikingState(AIContext context) : base(context) { }

        public override void OnEnterState() {
            ctx.TimeLastStruckPuck = -999;
        }

        public override AIMalletState UpdateState() {
            // if the puck has recently struck, it is stunlocked for a bit.
            if (OnStrikeCooldown()) return AIMalletState.Striking; 

            var shouldStrike = 
                ctx.WithinStrikingDistance
                && ctx.AIBehindPuck
                && (ctx.PuckMovingTowardsUs || ctx.PuckMovingAwayTooSlow);

            if(shouldStrike)
                return AIMalletState.Striking;

            // otherwise amble.
            return AIMalletState.Ambling;
        }

        public override Vector3 UpdatePosition() {
            // TODO: be smart about the interception point.
            //      if the player is not in between the puck and the goal, strike straight for the goal (can randomize the chance of this)
            //      else try to bounce it off either wall (45 degrees?).
             
            if (!OnStrikeCooldown()) 
                TriggerStrike(); 

            // execute the strike
            var percThroughStrike = TimeSinceLastStrike() / GameController.Instance.MalletAiStrikeTime;
            var animPercBetweenPoints = GameController.Instance.MalletAIStrikeCurve.Evaluate(percThroughStrike); 
            Debug.LogWarning($"Anim: {percThroughStrike:F3} {animPercBetweenPoints:F3}");
            return Vector3.Lerp(strikeReturnPos, strikeTgtPos, animPercBetweenPoints);
        }

        void TriggerStrike() {
            ctx.TimeLastStruckPuck = ctx.Time;
            var puckPosition = ctx.Puck.Rb.position;
            var puckVelocity = ctx.Puck.Rb.velocity;
            var malletPos = ctx.AiMallet.Rb.position;
            strikeTgtPos = puckPosition - puckVelocity.normalized * GameController.Instance.MalletAiStrikeForce;
            strikeReturnPos = malletPos;
            Debug.LogWarning($"Strike: {strikeTgtPos} {strikeReturnPos}");
        }

        bool OnStrikeCooldown() {
            return TimeSinceLastStrike() < GameController.Instance.MalletAiStrikeTime;
        }

        float TimeSinceLastStrike() {
            return ctx.Time - ctx.TimeLastStruckPuck;
        }
    }
}
