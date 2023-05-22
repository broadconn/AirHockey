using UnityEngine;

namespace Assets.Scripts.AI {
    /// <summary>
    /// Intercepting the puck, ideally to hit it back towards the player goal
    /// </summary>
    internal class InterceptingState : AIState {
        bool debugLogs = false;
        Vector3 inaccuracyOffset;

        public InterceptingState(AIContext context) : base(context) {

        }

        public override void OnEnterState() {
            //SetInaccuracy();
        }

        public override AIMalletState UpdateState() { 
            if (Ctx.AIBehindPuck && Ctx.WithinStrikingDistance)
                return AIMalletState.Striking;
               
            var shouldIntercept = Ctx.PuckMovingTowardsAI || (Ctx.PuckOnOurSide && Ctx.PuckMovingAwayTooSlow); 
            if (shouldIntercept)
                return AIMalletState.Intercepting;

            // otherwise just amble
            return AIMalletState.Ambling;
        }

        // move to interception point
        public override Vector3 UpdatePosition() {
            var aiPos = Ctx.AiMallet.Rb.position;

            // if the puck is moving away too slow and we should it a nudge but we're in front of the puck, we need to get behind it.
            if (Ctx.PuckMovingAwayTooSlow && !Ctx.AIBehindPuck) {
                //if(debugLogs) Debug.Log($"MOVING BEHIND THE PUCK");
                return Ctx.AIGoalPos; // dumb logic, lets hope it works most of the time 
            }

            //Sample a number of positions from the puck to the red end / goal
            var numExtraSamples = 4; // +1 for from the AI current position
            for (int i = 0; i <= numExtraSamples; i++) {
                // just lerp our position closer to the goal and call GetClosestPointAndTime again 
                var perc = i / numExtraSamples;
                var samplePos = Vector3.Lerp(aiPos, Ctx.AIGoalPos, perc); // TODO might be cheaper to sample down the actual path?

                var (closestPathInterceptionPoint, timeBeforePuckReaches) = Ctx.PuckFuturePath.GetClosestPointOnPath(samplePos);
                if (CanReachPointOnPath(closestPathInterceptionPoint, timeBeforePuckReaches)) {
                    //if (debugLogs) Debug.Log($"GOING TO INTERCEPTION POINT {i}");
                    Ctx.LastInterceptionTgtPoint = closestPathInterceptionPoint;
                    return closestPathInterceptionPoint + inaccuracyOffset;
                }
            }

            // if we can't intercept any of those points...

            // if it's going into the goal, head straight to the goal point ( pretend it cares )
            if (Ctx.PuckFuturePath.WillEnterGoal) {
                //if (debugLogs) Debug.Log("ITS GOING IN THE GOAL, I CANNOT REACH AHHH");
                return Ctx.AIGoalPos;
            }

            // else (can't intercept, not heading towards goal) head towards the impact point on our wall
            //if (debugLogs) Debug.Log($"Cant intercept. Going home / wall. {Ctx.Puck.Rb.velocity.z}");
            return Ctx.PuckFuturePath.RedWallImpactPoint ?? aiPos;
        } 

        // TODO consider returning more info on how well it can hit the puck
        // e.g. 0 if cant reach at all, 0.01 if can nick it, 1 if we have full control over where it will be able to hit
        bool CanReachPointOnPath(Vector3 point, float timeLeft) { 
            var curPos = Ctx.AiMallet.Rb.position;
            var distanceToTravel = Vector3.Distance(point, curPos);
            var speed = GameController.Instance.MalletAIMaxSpeed;
            return distanceToTravel / speed <= timeLeft;
        }

        void SetInaccuracy() { 
            var maxDeviation = Ctx.AiMallet.Radius;
            var xDeviation = Random.Range(-maxDeviation, maxDeviation);
            var zDeviation = Random.Range(-maxDeviation, maxDeviation);
            inaccuracyOffset = new Vector3(xDeviation, 0, zDeviation);
        }
    }
}
