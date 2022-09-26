using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour {
    private static GameController _instance;
    public static GameController Instance { get { return _instance; } }

    [Header("Puck Settings")]
    [SerializeField] float puckMaxSpeed = 20;
    public float PuckMaxSpeed { get => puckMaxSpeed; }

    [SerializeField] float puckMinSpeed = 2;
    public float PuckMinSpeed { get => puckMinSpeed; }

    [Header("Player Settings")]
    [SerializeField] float malletPlayerMaxSpeed = 40;
    public float MalletPlayerMaxSpeed { get => malletPlayerMaxSpeed; }

    [Header("AI Settings")]
    [SerializeField] float malletAIMaxSpeed = 10;
    public float MalletAIMaxSpeed { get => malletAIMaxSpeed; }

    [SerializeField] float malletAIMinSpeed = 0.5f;
    public float MalletAIMinSpeed { get => malletAIMinSpeed; }


    [Header("References")]
    [SerializeField] Puck puck;
    [SerializeField] PlayerMallet player;
    [SerializeField] AIMallet aiMallet;
    [SerializeField] Transform p1PuckSpawnPos;
    [SerializeField] Transform p2PuckSpawnPos;
    [SerializeField] ScoreText p1ScoreText;
    [SerializeField] ScoreText p2ScoreText;


    public int P1Score { get; private set; }
    public int P2Score { get; private set; }

    private void Awake() {
        _instance = _instance != null ? _instance : this;
    }

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
        ResetForNewRound();
    }

    public void PlayerScored(int playerNum) {
        P1Score += playerNum == 1 ? 1 : 0;
        P2Score += playerNum == 2 ? 1 : 0;
        UpdateScoreText();
        ResetForNewRound();
    } 

    void ResetForNewRound() {
        puck.ResetForNewRound(p1PuckSpawnPos.position);
        player.ResetForNewRound();
        aiMallet.ResetForNewRound();
    }

    void UpdateScoreText() {
        p1ScoreText.UpdateScore(P1Score);
        p2ScoreText.UpdateScore(P2Score);
    }
}
