using UnityEngine;

namespace Assets.Scripts.AI {
    internal abstract class AIState {
        protected readonly AIContext Ctx;

        protected AIState(AIContext context) {
            Ctx = context;
        }
        public abstract void OnEnterState();
        public abstract AIMalletState UpdateState();
        public abstract Vector3 UpdatePosition();
    }  
}
