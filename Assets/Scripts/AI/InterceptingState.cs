using UnityEngine;

namespace Assets.Scripts.AI {
    /// <summary>
    /// Intercepting the puck, ideally to hit it back towards the player goal
    /// </summary>
    internal class InterceptingState : AIState {

        public InterceptingState(AIContext context) : base(context) {

        }

        public override AIMalletState UpdateState() {
            // if close to the puck and behind it, strike it 
            if (ctx.AiMallet.Rb.position.z > ctx.Puck.Rb.position.z
                && ctx.AiDistFromPuck < GameController.Instance.MalletAiStrikeDistance)
                return AIMalletState.Striking;

            // if the puck is heading away, lose interest
            var puckTooSlowSpeed = -4f;
            var puckHeadingAway = ctx.Puck.Rb.velocity.z < 0;
            var puckMovingAwayTooSlow = false;// ctx.Puck.Rb.velocity.z < 0 && ctx.Puck.Rb.velocity.z > puckTooSlowSpeed;
            var loseInterest = puckHeadingAway && !puckMovingAwayTooSlow;
            if (loseInterest)
                return AIMalletState.Ambling;

            return AIMalletState.Intercepting;
        }

        // move to interception point
        public override Vector3 UpdatePosition() {
            var aiPos = ctx.AiMallet.Rb.position;
             
            // Sample a number of positions from the puck to the red end / goal
            var numExtraSamples = 4; // +1 for from the AI current position
            for(int i = 0; i <= numExtraSamples; i++) {
                // just lerp our position closer to the goal and call GetClosestPointAndTime again 
                var perc = i / numExtraSamples;
                var samplePos = Vector3.Lerp(aiPos, ctx.AIGoalPos, perc); // TODO might be cheaper to sample down the actual path?

                var (closestPathInterceptionPoint, timeBeforePuckReaches) = ctx.PuckFuturePath.GetClosestPointOnPath(samplePos);
                if (CanReachPointOnPath(closestPathInterceptionPoint, timeBeforePuckReaches)) {
                    return closestPathInterceptionPoint;
                }
            }

            // if we can't intercept any of those points...

            // if it's going into the goal, head straight to the goal point ( pretend it cares )
            if (ctx.PuckFuturePath.WillEnterGoal)
                return ctx.AIGoalPos; 

            // else (can't intercept, not heading towards goal) head towards the impact point on our wall
            return ctx.PuckFuturePath.FarWallImpactPoint ?? aiPos;
        }

        // TODO consider returning more info on how well it can hit the puck
        // e.g. 0 if cant reach at all, 0.01 if can touch it, 1 if we have full control over where it will be able to hit
        bool CanReachPointOnPath(Vector3 point, float timeLeft) { 
            var curPos = ctx.AiMallet.Rb.position;
            var distanceToTravel = Vector3.Distance(point, curPos);
            var speed = GameController.Instance.MalletAIMaxSpeed;
            return distanceToTravel / speed <= timeLeft;
        }
    }
}
