using Mono.Cecil;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts.AI {
    /// <summary>
    /// Puck is moving away, towards player or player hasn't hit the puck yet. 
    /// Do what real people do, hover randomly near the goal, or closer to the centerline if the player is closer to their goal.
    /// </summary>
    internal class AmblingState : AIState {   
        // random ambling settings
        Vector3 amble;
        float timeNextAmbleUpdate = 0;
        float ambleSeedX, ambleSeedY;
        const float minAmbleUpdateS = 0.1f, maxAmbleUpdateS = 3f;
        const float maxAmbleRange = 1f;
        Vector3 ambleCenter;

        public AmblingState(AIContext context) : base(context) {
            ambleSeedX = Random.Range(0, 1000);
            ambleSeedY = Random.Range(0, 1000);
        }

        public override AIMalletState UpdateState() {
            // if the puck is heading towards us, change to interception mode.
            var lowestVelToStllChasePuck = 4f;
            var puckIsMoving = ctx.Puck.Rb.velocity.magnitude > 0;
            var puckHeadingTowardsMe = ctx.Puck.Rb.velocity.z > 0;
            var puckMovingAwayTooSlow = false;// ctx.Puck.Rb.velocity.z < 0 && ctx.Puck.Rb.velocity.z > -lowestVelToStllChasePuck; 
            if (puckIsMoving && (puckHeadingTowardsMe || puckMovingAwayTooSlow))
                return AIMalletState.Intercepting;

            return AIMalletState.Ambling;
        }

        public override Vector3 UpdatePosition() {
            amble = UpdateSmoothAmble();

            ambleCenter = GetAmbleCenter();

            return ambleCenter + amble;
        }

        float ConfidenceToAmbleRange() {
            return -1;
        }

        Vector3 UpdateRandomAmble(Vector3 curAmble) { 
            if (ctx.Time > timeNextAmbleUpdate) {
                var ambleAmount = Random.Range(0f, maxAmbleRange);
                curAmble = new Vector3(Mathf.PerlinNoise(ambleSeedX, ctx.Time) - 0.5f, 0, Mathf.PerlinNoise(ctx.Time, ambleSeedY) - 0.5f).normalized * ambleAmount;
                timeNextAmbleUpdate = ctx.Time + Random.Range(minAmbleUpdateS, maxAmbleUpdateS); 
            }
            return curAmble;
        }

        Vector3 UpdateSmoothAmble() {
            var t = Time.time;
            var oscillateTime = 0.5f;
            var xWid = Mathf.Lerp(GameController.Instance.MalletAIAmbleX.x, GameController.Instance.MalletAIAmbleX.y, ctx.Confidence.Value);
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
            var ambleLocation = Vector3.Lerp(unconfidentEnd, confidentEnd, ctx.Confidence.Value);

            // dont go ahead of the puck, it's weird bro 
            if (ctx.Puck.Rb.position.z > ambleLocation.z) {
                ambleLocation = new Vector3(ambleLocation.x, ambleLocation.y, ctx.Puck.Rb.position.z + ctx.AiMallet.Radius);
            }

            return ambleLocation;// ctx.AiMallet.Rb.position;
        }
    }
}
