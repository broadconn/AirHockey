using UnityEngine;

namespace Assets.Scripts.AI {
    internal interface IAIState {
        public Vector3 UpdatePosition(AIMallet aiMallet, Puck puck, PlayerMallet player);
        public void UpdateState(AIMallet aiMallet, Puck puck, PlayerMallet player);
    }

    internal class PausedState : IAIState {
        public Vector3 UpdatePosition(AIMallet aiMallet, Puck puck, PlayerMallet player) {
            // don't move :)
            return aiMallet.Rb.position;
        }

        public void UpdateState(AIMallet aiMallet, Puck puck, PlayerMallet player) {

        }
    }

    /// <summary>
    /// Intercepting the puck, ideally to hit it back towards the player goal
    /// </summary>
    internal class AmblingState : IAIState {
        public Vector3 UpdatePosition(AIMallet aiMallet, Puck puck, PlayerMallet player) {
            // if we're already on the interception path with the puck
            //      do nothing, or move along the interception path towards the puck
            // else
            //   if puck will go into the goal
            //      go fast into a late interception point
            //   else
            //      
            return aiMallet.Rb.position;
        }

        public void UpdateState(AIMallet aiMallet, Puck puck, PlayerMallet player) {

        }
    }

    /// <summary>
    /// Intercepting the puck, ideally to hit it back towards the player goal
    /// </summary>
    internal class InterceptingState : IAIState {
        public Vector3 UpdatePosition(AIMallet aiMallet, Puck puck, PlayerMallet player) {
            // if we're already on the path, 
            // if puck will go into the goal, go fast into a late interception point
            return aiMallet.Rb.position;
        }

        public void UpdateState(AIMallet aiMallet, Puck puck, PlayerMallet player) {

        }
    }

    /// <summary>
    /// Strike at the puck, only change state once we have "recovered" from striking the puck.
    /// </summary>
    internal class StrikingState : IAIState {
        public Vector3 UpdatePosition(AIMallet aiMallet, Puck puck, PlayerMallet player) {
            // head towards the puck with max speed.
            // be smart about the interception point.
            //      if the player is not in between the puck and the goal, strike straight for the goal (can randomize the chance of this)
            //      else try to bounce it off either wall (45 degrees?).
            return aiMallet.Rb.position;
        }

        public void UpdateState(AIMallet aiMallet, Puck puck, PlayerMallet player) {

        }
    }
}
