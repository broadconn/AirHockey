﻿using UnityEngine;

namespace Assets.Scripts.AI { 
    internal class PausedState : AIState {
        public PausedState(AIContext context) : base(context) { }

        public override Vector3 UpdatePosition() {
            return ctx.AiMallet.Rb.position;
        }

        public override AIMalletState UpdateState() {
            return AIMalletState.Paused; // paused state is controlled externally
        }
    }
}