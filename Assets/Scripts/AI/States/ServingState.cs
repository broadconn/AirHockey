﻿using UnityEngine;

namespace Assets.Scripts.AI {
    internal class ServingState : AIState {
        bool debugLogs = false;
        bool hasGoneThroughStrikeLocation = false;
        bool hasGoneToServeStartLocation = false;
        bool hasGoneToServeStrikeLocation = false;
        
        const float distFromPuckToServe = 1f;
        Vector3 servePreStrikeLocation;
        Vector3 serveStrikeLocation;
        const float maxServeAngle = 70;

        public ServingState(AIContext context) : base(context) { }

        public override void OnEnterState() {
            hasGoneToServeStartLocation = false;
            hasGoneToServeStrikeLocation = false;
            hasGoneThroughStrikeLocation = false;
            SetRandomServeStrikePos();
        }

        void SetRandomServeStrikePos() {
            var puckSpawnPos = Ctx.Puck.AiPuckSpawnPos;
            var serveAngle = Random.Range(-maxServeAngle, maxServeAngle); 
            servePreStrikeLocation = GetPosAtOrbitAngle(puckSpawnPos, serveAngle, distFromPuckToServe);

            serveStrikeLocation = puckSpawnPos + (puckSpawnPos - servePreStrikeLocation).normalized * 0.05f;
        }

        Vector3 GetPosAtOrbitAngle(Vector3 orbitPos, float angle, float dist) {
            var x = Mathf.Sin(angle * Mathf.Deg2Rad);
            var z = Mathf.Cos(angle * Mathf.Deg2Rad);
            var orbit = new Vector3(x, 0, z).normalized * dist;
            return orbitPos + orbit;
        }

        public override AIMalletState UpdateState() {
            if (hasGoneThroughStrikeLocation) {
                if (debugLogs) Debug.Log("hit puck, going dark");
                return AIMalletState.Ambling;
            }

            return AIMalletState.Serving;
        }

        public override Vector3 UpdatePosition() {
            // go to start of serve pos
            if (!hasGoneToServeStartLocation) {
                var distFromServePos = Vector3.Distance(Ctx.AIServeStartPos, Ctx.AiMallet.Rb.position);
                if(debugLogs) Debug.Log($"going to serve start {Ctx.AIServeStartPos} {Ctx.AiMallet.Rb.position} {distFromServePos:F2}");
                if (distFromServePos > 0.1f)
                    return Ctx.AIServeStartPos;

                hasGoneToServeStartLocation = true;
            }

            // go to a point near the puck at an angle
            if (!hasGoneToServeStrikeLocation) {
                if (debugLogs) Debug.Log($"going to pre-strike pos {servePreStrikeLocation}");
                var distFromStrikePos = Vector3.Distance(Ctx.AiMallet.Rb.position, servePreStrikeLocation);
                if (distFromStrikePos > 0.1f)
                    return servePreStrikeLocation;

                hasGoneToServeStrikeLocation = true;
            }

            // do the hit
            if (!hasGoneThroughStrikeLocation) {
                if (debugLogs) Debug.Log($"going to strike location {servePreStrikeLocation}");
                var distFromStrikePos = Vector3.Distance(Ctx.AiMallet.Rb.position, serveStrikeLocation);
                if (distFromStrikePos > 0.1f)
                    return serveStrikeLocation;

                hasGoneThroughStrikeLocation = true;
            }
              
            return serveStrikeLocation;
        }
    }
}