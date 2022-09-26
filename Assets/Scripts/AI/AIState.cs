using UnityEditor.Rendering.LookDev;
using UnityEngine;

namespace Assets.Scripts.AI {
    internal abstract class AIState {
        protected AIContext ctx;

        public AIState(AIContext context) {
            ctx = context;
        } 
        public abstract AIMalletState UpdateState();
        public abstract Vector3 UpdatePosition();
    }  
}
