using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class Fade : MonoBehaviour {

    [SerializeField]
    private MaskableGraphic whiteOverfade;

    [SerializeField]
    private float fadeTime = 0.5f;

    [SerializeField]
    private float fadeBlankWait = 0.5f;

    [SerializeField]
    private bool initialFade = false;

    public bool IsAnimating { get { return currentFade != null && isInFirstHalfOfFading; } }

    private bool isInFirstHalfOfFading = false;

    private Coroutine currentFade;

    void Awake()
    {
        if (initialFade)
        {
            FadeTransition(null,true);
        }
    }

    public void FadeTransition(Action middleCallback = null, bool fast = true)
    {
        if (currentFade != null)
        {
            StopCoroutine(currentFade);
            currentFade = null;
        }

        currentFade = StartCoroutine(FadeRoutine(middleCallback, fast));
    }

    IEnumerator FadeRoutine(Action middleCallback, bool fast)
    {
        isInFirstHalfOfFading = true;
        Color color = whiteOverfade.color;

        if (fast)
        {
            color.a = 1f;
            whiteOverfade.color = color;
        }
        else
        {
            while (whiteOverfade.color.a < 1f)
            {
                color.a += Time.deltaTime / fadeTime;
                whiteOverfade.color = color;
                yield return null;
            }
        }

        if (middleCallback != null)
        {
            middleCallback();
        }

        float time = Time.time + fadeBlankWait;
        while(time > Time.time)
        {
            yield return null;
        }

        isInFirstHalfOfFading = false;

        while (whiteOverfade.color.a > 0f)
        {
            color.a -= Time.deltaTime / fadeTime;
            whiteOverfade.color = color;
            yield return null;
        }

        currentFade = null;
    }

}
