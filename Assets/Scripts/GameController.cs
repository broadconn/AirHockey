using System;
using UnityEngine;

public class GameController : MonoBehaviour {
    private static GameController _instance;
    public static GameController Instance { get { return _instance; } }
    public event EventHandler<GoalScoredEventArgs> GoalScoredEvent;

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

    [SerializeField] float malletAiMaxSpeedToStillChasePuck = 4f;
    public float MalletAiMaxSpeedToStillChasePuck { get => malletAiMaxSpeedToStillChasePuck; }

    [SerializeField] float malletAiStrikeDistance = 0.5f;
    public float MalletAiStrikeDistance { get => malletAiStrikeDistance; }

    [SerializeField] float malletAiStrikeForce = 5f;
    public float MalletAiStrikeForce { get => malletAiStrikeForce; }

    [SerializeField] float malletAiStrikeTime = 0.5f;
    public float MalletAiStrikeTime { get => malletAiStrikeTime; }

    [SerializeField] AnimationCurve malletAIStrikeCurve;
    public AnimationCurve MalletAIStrikeCurve { get => malletAIStrikeCurve; }


    /// <summary>
    /// Amble range at low confidence (close to the goal) vs when confidence is high
    /// </summary>
    [SerializeField] Vector2 malletAIAmbleX = new(0.1f, 1f);
    public Vector2 MalletAIAmbleX { get => malletAIAmbleX; }

    [SerializeField] float malletAIAmbleY = 0.1f;
    public float MalletAIAmbleY { get => malletAIAmbleY; } 

    [Header("References")]
    [SerializeField] Puck puck;
    [SerializeField] PlayerMallet player;
    [SerializeField] AIMallet aiMallet;
    [SerializeField] Transform p1PuckSpawnPos;
    [SerializeField] Transform p2PuckSpawnPos;
    [SerializeField] ScoreText p1ScoreText;
    [SerializeField] ScoreText p2ScoreText;

    [Header("Debug")]
    public bool Debug = false;

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
        CheckForDebugChange();
    }

    void CheckForDebugChange() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            Debug = !Debug;
        }
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

        var e = new GoalScoredEventArgs(playerNum);
        GoalScoredEvent?.Invoke(this, e);

        ResetForNewRound(); // consider doing this once the player has clicked a ui button
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

public class GoalScoredEventArgs : EventArgs {
    public int PlayerNum { get; set; }

    public GoalScoredEventArgs(int playerNum) {
        PlayerNum = playerNum;  
    }
}