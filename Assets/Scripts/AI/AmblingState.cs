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

        public AmblingState(AIContext context) : base(context) { 
        }

        public override void OnEnterState() {
            curCenter = ctx.AiMallet.Rb.position;
        }

        public override AIMalletState UpdateState() {
            // if the puck is heading towards us, change to interception mode. 
            var puckIsMoving = ctx.Puck.Rb.velocity.magnitude > 0;
            if (puckIsMoving && (ctx.PuckMovingTowardsAI || (ctx.PuckOnOurSide && ctx.PuckMovingAwayTooSlow)))
                return AIMalletState.Intercepting;

            return AIMalletState.Ambling;
        }

        public override Vector3 UpdatePosition() {
            amble = UpdateAmbleDiff();
            ambleCenter = GetAmbleCenter();
            curCenter = ambleCenter;// Vector3.Lerp(curCenter, ambleCenter, Time.deltaTime * speedReachCenterPoint);

            return curCenter + amble;
        }

        Vector3 UpdateAmbleDiff() {
            var t = Time.time;
            var oscillateTime = 0.5f;
            var xWid = Mathf.Lerp(GameController.Instance.MalletAIAmbleX.x, GameController.Instance.MalletAIAmbleX.y, ctx.Riskyness.Value);
            var yWid = GameController.Instance.MalletAIAmbleY;
            var x = Mathf.Sin(t / oscillateTime ) * xWid;
            var y = Mathf.Sin(t / oscillateTime * 2) * yWid;
            return new Vector3(x, 0, y);
        }

        Vector3 GetAmbleCenter() {
            // if the player scored last, position self close to the middle of the board. make it harder for the player. 
            // else maybe do the same but slower. Or just meander around where u hit the puck
            // confidence controls how far towards the red/blue centerline or goal we be feelin 

            var offset = 0.5f;
            var unconfidentEnd = ctx.AIGoalPos + (ctx.ArenaCenterPos - ctx.AIGoalPos).normalized * offset;
            var confidentEnd = ctx.ArenaCenterPos + (ctx.AIGoalPos - ctx.ArenaCenterPos).normalized * offset;
            var ambleLocation = Vector3.Lerp(unconfidentEnd, confidentEnd, ctx.Riskyness.Value);

            // dont go ahead of the puck, it's weird bro 
            if (ctx.Puck.Rb.position.z > ambleLocation.z) {
                ambleLocation = new Vector3(ambleLocation.x, ambleLocation.y, ctx.Puck.Rb.position.z + ctx.AiMallet.Radius);
            }

            return ambleLocation;// ctx.AiMallet.Rb.position;
        }
    }
}
