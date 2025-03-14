using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ScoreAnimation : MonoBehaviour {

    private struct Score
    {
        public bool almostWon ;
        public int playerA;
        public int playerB;
    }

    [SerializeField]
    UnityEngine.UI.Text textMesh;

    [SerializeField]
    UnityEngine.UI.Outline outline;

    [SerializeField]
    private float waitBeforeScore = 0.5f;

    [SerializeField]
    private float blinkSpeed = 0.1f;

    [SerializeField]
    private AudioClip[] scoreNotes = new AudioClip[3];

    public bool IsAnimating { get { return currentAnim != null && scoresToDisplay.Count == 0; } }

    private Level currentLevel;

    private Coroutine currentAnim;

    private Queue<Score> scoresToDisplay = new Queue<Score>();

    void Start()
    {
        textMesh.enabled = false;
        Game.i.OnLevelLoaded += I_OnLevelLoaded;

        if (Game.i.Level != null)
        {
            I_OnLevelLoaded();
        }
    }

    private void I_OnLevelLoaded()
    {
        if (currentLevel != null)
        {
            currentLevel.OnScoreChanged -= CurrentLevel_OnScoreChanged;
        }

        currentLevel = Game.i.Level;
        currentLevel.OnScoreChanged += CurrentLevel_OnScoreChanged;
    }

    void OnDisable()
    {
        if (currentAnim != null)
        {
            StopCoroutine(currentAnim);
            currentAnim = null;
        }
    }

    private void CurrentLevel_OnScoreChanged()
    {
        if (!Game.i.Level.GameOver)
        {
            scoresToDisplay.Enqueue(new Score()
            {
                playerA = Game.i.Level.Scores[0],
                playerB = Game.i.Level.Scores[1],
                almostWon = Game.i.Level.HasAlmostWon()
            });
        }
    }

    void Update()
    {
        if (currentAnim == null && scoresToDisplay.Count > 0)
        {
            Score toAnimate = scoresToDisplay.Dequeue();
            currentAnim = StartCoroutine(AnimateScoreCoroutine(toAnimate));
        }
    }

    private IEnumerator AnimateScoreCoroutine(Score scoreToAnimate)
    {
        Color almostWonColor = Color.red;

        if (scoreToAnimate.almostWon)
        {
            outline.effectColor = almostWonColor;
        }
        else
        {
            outline.effectColor = Color.black;
        }

        textMesh.enabled = true;
        textMesh.text = "\n\n";

        yield return new WaitForSeconds(waitBeforeScore);

        Game.i.GeneralAudioSource.PlayOneShot(scoreNotes[0]);
        textMesh.text = scoreToAnimate.playerA + "\n\n";
        yield return new WaitForSeconds(waitBeforeScore);

        Game.i.GeneralAudioSource.PlayOneShot(scoreNotes[1]);
        textMesh.text = scoreToAnimate.playerA + "\n.\n";
        yield return new WaitForSeconds(waitBeforeScore);

        Game.i.GeneralAudioSource.PlayOneShot(scoreNotes[2]);
        textMesh.text = scoreToAnimate.playerA + "\n.\n"+scoreToAnimate.playerB;
        yield return new WaitForSeconds(waitBeforeScore * 2);

        float time = Time.time;
        bool first = false;
        while (scoresToDisplay.Count > 0 || first)
        {
            first = false;

            while (Time.time < time + waitBeforeScore)
            {
                textMesh.text = scoreToAnimate.playerA + "\n.\n" + scoreToAnimate.playerB;
                textMesh.enabled = !textMesh.enabled;

                yield return new WaitForSeconds(blinkSpeed);
            }

            if (scoresToDisplay.Count > 0)
            {
                scoreToAnimate = scoresToDisplay.Dequeue();

                if (scoreToAnimate.almostWon)
                {
                    outline.effectColor = almostWonColor;
                }
                else
                {
                    outline.effectColor = Color.black;
                }
            }
        }

        currentAnim = null;
        textMesh.enabled = false;
    }
}
