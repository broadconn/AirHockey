using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{ 
    [SerializeField] AnimationCurve scaleCurve;

    TextMeshProUGUI scoreText;
    float timeScored = -999;
    float scaleAnimLength = 1;
    float curScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        scoreText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        float scaleProg = (Time.time - timeScored) / scaleAnimLength;
        float scaleMod = scaleCurve.Evaluate(scaleProg);
        transform.localScale = new Vector3(1 + scaleMod, 1 + scaleMod, 1 + scaleMod);
    }

    public void UpdateScore(float newScore) {
        if (curScore != newScore) {
            timeScored = Time.time;
            //play fireworks
        }

        scoreText.text = newScore.ToString();
        curScore = newScore;
    }
}
