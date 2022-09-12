using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {
    [SerializeField] GameController gameControl;
    [SerializeField] int otherPlayerNum;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Puck")) {
            gameControl.PlayerScored(otherPlayerNum);
        }
    }
}
