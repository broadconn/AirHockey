using UnityEngine;

namespace Assets.Scripts.AI {
    /// <summary>
    /// Strike at the puck, only change state once we have "recovered" from striking the puck.
    /// </summary>
    internal class StrikingState : AIState {
        bool debugLogs = false;
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
                && (ctx.PuckMovingTowardsAI || ctx.PuckMovingAwayTooSlow);

            if(shouldStrike)
                return AIMalletState.Striking;

            return AIMalletState.Ambling;
        }

        public override Vector3 UpdatePosition() {
            if (!OnStrikeCooldown()) 
                TriggerStrike(); 

            // execute the strike
            var percThroughStrike = TimeSinceLastStrike() / GameController.Instance.MalletAiStrikeTime;
            var animPercBetweenPoints = GameController.Instance.MalletAIStrikeCurve.Evaluate(percThroughStrike); 
            return Vector3.Lerp(strikeReturnPos, strikeTgtPos, animPercBetweenPoints);
        }

        void TriggerStrike() {
            ctx.TimeLastStruckPuck = ctx.Time;
            var puckPosition = ctx.Puck.Rb.position;
            var puckVelocity = ctx.Puck.Rb.velocity;
            var malletPos = ctx.AiMallet.Rb.position;

            // TODO: be smart about the interception point.
            //      if the player is not in between the puck and the goal, aim straight for the goal (can randomize the chance of this)
            //      else try to bounce it off either wall (45 degrees?).

            //strikeTgtPos = puckPosition - puckVelocity.normalized * GameController.Instance.MalletAiStrikeForce; // this doesn't work well if the mallet isn't in the interception path already.

            // TODO: improve the strike location if the mallet isn't on the interception path yet
            //var puckFutureProject = 0;// 0.1f; // try to get where the puck will be when we hit it
            //var futurePuckPos = puckPosition + puckVelocity.normalized * puckFutureProject;
            //strikeTgtPos = futurePuckPos + (futurePuckPos - malletPos).normalized * GameController.Instance.MalletAiStrikeForce;

            // strike direction = halfway between the interception point and the puck? puck should be p close to the intercept point
            // figure out the actual math based on puck speed if this isn't good enough
            var closestPathPos = ctx.PuckFuturePath.GetClosestPointOnPath(malletPos).point;
            var strikeTgt = Vector3.Lerp(closestPathPos, puckPosition, 0.1f);
            strikeTgtPos = strikeTgt + (strikeTgt - malletPos).normalized * GameController.Instance.MalletAiStrikeForce;

            strikeReturnPos = malletPos;
            if (debugLogs) Debug.Log($"Strike: {ctx.LastInterceptionTgtPoint} {puckPosition} {strikeTgtPos} {strikeReturnPos}");
        }

        bool OnStrikeCooldown() {
            return TimeSinceLastStrike() < GameController.Instance.MalletAiStrikeTime;
        }
         
        float TimeSinceLastStrike() {
            return ctx.Time - ctx.TimeLastStruckPuck;
        }
    }
}
