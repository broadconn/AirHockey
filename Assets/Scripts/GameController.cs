using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour {
    [SerializeField] Puck puck;
    [SerializeField] Transform p1PuckSpawnPos;
    [SerializeField] Transform p2PuckSpawnPos;
    [SerializeField] ScoreText p1ScoreText;
    [SerializeField] ScoreText p2ScoreText;

    public int P1Score { get; private set; }
    public int P2Score { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        PrepareForNewGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PrepareForNewGame() {
        P1Score = 0;
        P2Score = 0;
        SpawnPuck(1);
    }

    public void PlayerScored(int playerNum) {
        P1Score += playerNum == 1 ? 1 : 0;
        P2Score += playerNum == 2 ? 1 : 0;
        UpdateScoreText();
        SpawnPuck(1);
    }

    void SpawnPuck(int playerNum) {
        puck.ResetToPosition(playerNum == 1 ? p1PuckSpawnPos.position : p2PuckSpawnPos.position);
    }

    void UpdateScoreText() {
        p1ScoreText.UpdateScore(P1Score);
        p2ScoreText.UpdateScore(P2Score);
    }
}
