using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    [SerializeField] float bobMag = 0.2f;
    [SerializeField] float bobSpeed = 0.2f;
    [SerializeField] float bobOffset = 0;
    [SerializeField] AnimationCurve scaleCurve;
    TextMeshProUGUI scoreText;
    Vector3 startPos;
    float timeScored = -999;
    float scaleAnimLength = 1;
    float curScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        scoreText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(startPos.x, startPos.y + Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobMag, startPos.z);

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
