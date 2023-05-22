using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GameController : MonoBehaviour {
    public static GameController Instance { get; private set; }

    public event EventHandler<GoalScoredEventArgs> GoalScoredEvent;

    [Header("Puck Settings")]
    [SerializeField] private float puckMaxSpeed = 20;
    public float PuckMaxSpeed => puckMaxSpeed;

    [SerializeField] private float puckMinSpeed = 2;
    public float PuckMinSpeed => puckMinSpeed;

    [Header("Player Settings")]
    [SerializeField] private float malletPlayerMaxSpeed = 40;
    public float MalletPlayerMaxSpeed => malletPlayerMaxSpeed;

    [Header("AI Settings")]
    [SerializeField] private float malletAIMaxSpeed = 10;
    public float MalletAIMaxSpeed => malletAIMaxSpeed;

    [SerializeField] private float malletAiMaxSpeedToStillChasePuck = 4f;
    public float MalletAiMaxSpeedToStillChasePuck => malletAiMaxSpeedToStillChasePuck;

    [SerializeField] private float malletAiStrikeDistance = 0.5f;
    public float MalletAiStrikeDistance => malletAiStrikeDistance;

    [SerializeField] private float malletAiStrikeForce = 5f;
    public float MalletAiStrikeForce => malletAiStrikeForce;

    [SerializeField] private float malletAiStrikeTime = 0.5f;
    public float MalletAiStrikeTime => malletAiStrikeTime;

    [SerializeField] private AnimationCurve malletAIStrikeCurve;
    public AnimationCurve MalletAIStrikeCurve => malletAIStrikeCurve;

    private int LastPlayerScored { get; set; }


    /// <summary>
    /// Amble range at low confidence (close to the goal) vs when confidence is high
    /// </summary>
    [SerializeField] private Vector2 malletAIAmbleX = new(0.1f, 1f);
    public Vector2 MalletAIAmbleX => malletAIAmbleX;

    [SerializeField] private float malletAIAmbleY = 0.1f;
    public float MalletAIAmbleY => malletAIAmbleY;

    [Header("References")]
    [SerializeField] private Puck puck;
    [SerializeField] private PlayerMallet player;
    [SerializeField] private AIMallet aiMallet;
    [SerializeField] private Transform p1PuckSpawnPos;
    [SerializeField] private Transform p2PuckSpawnPos;
    [SerializeField] private ScoreText p1ScoreText;
    [SerializeField] private ScoreText p2ScoreText;

    [Header("Debug")]
    public bool debug = false;

    public int P1Score { get; private set; }
    public int P2Score { get; private set; }

    private void Awake() {
        Instance = Instance != null ? Instance : this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        PrepareForNewGame();
    }

    // Update is called once per frame
    private void Update()
    {
        CheckForDebugChange();
    }

    private void CheckForDebugChange() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            debug = !debug;
        }
    }

    private void PrepareForNewGame() {
        P1Score = 0;
        P2Score = 0;
        LastPlayerScored = 2; // set to opposite of the player that should start
        ResetForNewRound();
    }

    public void PlayerScored(int playerNum) {
        P1Score += playerNum == 1 ? 1 : 0;
        P2Score += playerNum == 2 ? 1 : 0;
        LastPlayerScored = playerNum;
        UpdateScoreText();

        var e = new GoalScoredEventArgs(playerNum);
        GoalScoredEvent?.Invoke(this, e);

        ResetForNewRound();
    } 

    private void ResetForNewRound() {
        var playerServing = LastPlayerScored != 1;
        puck.ResetForNewRound(playerServing);
        aiMallet.ResetForNewRound(!playerServing);
    }

    private void UpdateScoreText() {
        p1ScoreText.UpdateScore(P1Score);
        p2ScoreText.UpdateScore(P2Score);
    }
}

public class GoalScoredEventArgs : EventArgs {
    public int PlayerNum { get; set; }

    public GoalScoredEventArgs(int playerNum) {
        PlayerNum = playerNum;  
    }
}