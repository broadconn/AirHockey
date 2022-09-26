using UnityEngine;

namespace Assets.Scripts.AI {
    /// <summary>
    /// Intercepting the puck, ideally to hit it back towards the player goal
    /// </summary>
    internal class InterceptingState : AIState {
        float malletAIStrikeDistance = 0.5f;

        public InterceptingState(AIContext context) : base(context) {

        }

        public override AIMalletState UpdateState() {
            // if close to the puck, strike it 
            if (ctx.AiDistFromPuck < malletAIStrikeDistance)
                return AIMalletState.Striking;

            // if the puck is heading away, lose interest
            var loseInterest = ctx.PuckDirectionAngle > 90;
            if (loseInterest)
                return AIMalletState.Ambling;

            return AIMalletState.Intercepting;
        }

        public override Vector3 UpdatePosition() {

            // old dumb logic. Just try to go to behind the puck.
            //var malletBehindPosDist = 2f;
            //var puckPosition = ctx.Puck.Rb.position;
            //var puckVelocity = ctx.Puck.Rb.velocity;
            //var futurePuckPos = puckPosition + puckVelocity.normalized * malletBehindPosDist;
            //return futurePuckPos;

            // NEW SMORT LOGIC
            // if we're already on the interception path with the puck
            //      do nothing, or move along the interception path towards the puck
            // else
            //   if puck will go into the goal
            //      go fast into a late interception point
            //   else
            //      if we can make it to the interception path in time
            //          go there
            //      else
            //          avoid the puck. (in the future we can try to catch the puck as it bounces back?)


            // TESTING STUFF
            // move to interception point
            var aiPos = ctx.AiMallet.Rb.position;
            return ctx.PuckFuturePath.GetClosestPoint(aiPos);
        }
    }
}
