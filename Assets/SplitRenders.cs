﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SplitRenders : MonoBehaviour
{
    [SerializeField]
    private float splitSize = 1f;

    [SerializeField]
    private bool manualFlipControl = false;

    [SerializeField]
    [Range(0f, 1f)]
    private float flip = 0.5f;

    [SerializeField]
    private float excenteredAmountV = 0.7f;

    [SerializeField]
    private float excenteredAmountH = 0.5f;

    [SerializeField]
    private float verticalVisibilityOffset = 0.1f;

    [SerializeField]
    private CanvasScaler canvasScaler;

    [SerializeField]
    private RectTransform topTransform;

    [SerializeField]
    private RectTransform maskTransform;

    [SerializeField]
    private RectTransform childMaskTransform;

    public float HorizontalAmount { get { return Mathf.Sin(flip * Mathf.PI); } }

    private Vector2 CanvasSize { get { return canvasScaler.referenceResolution; } }

    private Transform TransformA { get { return transforms[0]; } }
    private Transform TransformB { get { return transforms[1]; } }

    private Transform[] transforms;

    private float flipTarget = 0f;

    private bool isLocked = false;

    public void SetTrackingTargets(Transform[] targets)
    {
        transforms = targets;
        ComputeSplit();

        flip = flipTarget;
        UpdateSplit();
    }

    public void Lock(float? flipTarget = null)
    {
        isLocked = true;
        this.flipTarget = flipTarget.HasValue ? flipTarget.Value : this.flipTarget;
    }

    public void Unlock()
    {
        isLocked = false;
    }

    void Update()
    {
        if (transforms != null)
        {
            if (!isLocked)
            {
                ComputeSplit();
            }

            flip = Mathf.Lerp(flip, flipTarget, Time.deltaTime * 6f);

            UpdateSplit();
        }
    }

    void ComputeSplit()
    {
        if (!manualFlipControl)
        {
            flipTarget = Mathf.Clamp01((TransformB.position.y - TransformA.position.y) * 0.01f * splitSize + 0.5f);
        }
    }

#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    void UpdateSplit()
    {
        float rot = Mathf.PI * flip + Mathf.PI / 2;
        float sin = Mathf.Sin(rot);


        {
            float cos = Mathf.Cos(rot);

            float vOffset = Mathf.Clamp01(sin);

            topTransform.anchoredPosition = 
                Vector2.Scale(new Vector2(cos * 0.5f, sin * 0.5f), 0.5f * CanvasSize)
                + Vector2.down * vOffset * verticalVisibilityOffset * CanvasSize.y;
        }

        {
            maskTransform.anchoredPosition = Vector2.zero;
            maskTransform.localEulerAngles = new Vector3(0f, 0f, (Mathf.PI * flip) * Mathf.Rad2Deg);

            childMaskTransform.localEulerAngles = -maskTransform.localEulerAngles;

            float vOffset = Mathf.Clamp01(-sin);

            childMaskTransform.anchoredPosition = 
                Vector2.Scale(new Vector2(0f, excenteredAmountH * HorizontalAmount + excenteredAmountV * (1f-HorizontalAmount)), CanvasSize) +
                Vector2.up * vOffset * verticalVisibilityOffset * CanvasSize.y;

        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        UpdateSplit();

    }
#endif
}
