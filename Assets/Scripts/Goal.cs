using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {
    [SerializeField] int otherPlayerNum;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Puck")) {
            GameController.Instance.PlayerScored(otherPlayerNum);
        }
    }
}
