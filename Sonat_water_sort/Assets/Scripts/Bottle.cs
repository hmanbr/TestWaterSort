using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Bottle : MonoBehaviour
{
    // Events
    public event Action<Bottle, Bottle, bool> OnPourComplete;
    public event Action<Bottle> OnBottleComplete;

    [SerializeField] Transform bottleTransform;
    [SerializeField] Renderer bottleRenderer;

    [SerializeField] ColorPaletteSO palette;
    [SerializeField] int[] setColors;

    [SerializeField] float pourDuration = 3.5f;
    [SerializeField] float waterFillAndDrainDuration = 0.6f;

    const int k_bottleCapacity = 4;

    List<int> currentColorsList = new List<int>(k_bottleCapacity);

    MaterialPropertyBlock materialPropertyblock;

    int fillAmountID;
    int scaleAndRotationMultiplierID;

    int color01ID;
    int color02ID;
    int color03ID;
    int color04ID;

    float currentFillAmount = 1f;
    float scaleAndRotationMultiplier = 1f;

    bool isAnimating;

    public bool isComplete { get; private set; }

    // select source bottle tween (Pop bottle up)
    [SerializeField] float selectOffsetY = 0.4f;
    [SerializeField] float selectDuration = 0.2f;

    private Vector3 originalLocalPosition;
    private Tween selectTween;

    // source bottle move above target bottle, then pour
    [SerializeField] float pourHeightOffset = 5f;
    [SerializeField] float pourHorizontalOffset = 0.35f;
    [SerializeField] float moveDuration = 0.25f;

    private Vector3 originalPosition;

    [SerializeField] private Transform bottleMouthTransform;

    public Transform GetBottleMouthTransform()
    {
        return bottleMouthTransform;
    }

    void Awake()
    {
        originalLocalPosition = bottleTransform.localPosition;
        originalPosition = bottleTransform.position;

        // Get shader graph varibles
        materialPropertyblock = new MaterialPropertyBlock();

        fillAmountID = Shader.PropertyToID("_FillAmount");
        scaleAndRotationMultiplierID = Shader.PropertyToID("_ScaleAndRotationMultiplier");

        color01ID = Shader.PropertyToID("_Color01");
        color02ID = Shader.PropertyToID("_Color02");
        color03ID = Shader.PropertyToID("_Color03");
        color04ID = Shader.PropertyToID("_Color04");

        bottleRenderer.GetPropertyBlock(materialPropertyblock);
        UpdateShader();
    }

    private void Start()
    {

    }

    // Set bottle's color
    public void InitializeFromPalette(int[] setColors)
    {
        currentColorsList.Clear();

        foreach (var id in setColors)
        {
            if (id < 0) // empty slot
            {
                continue;
            }
            currentColorsList.Add(id);
        }

        UpdateShader();
    }

    void UpdateShader()
    {
        bottleRenderer.GetPropertyBlock(materialPropertyblock);

        float fill = currentColorsList.Count / (float)k_bottleCapacity;

        materialPropertyblock.SetFloat(fillAmountID, fill);
        materialPropertyblock.SetFloat(scaleAndRotationMultiplierID, scaleAndRotationMultiplier);

        materialPropertyblock.SetColor(color01ID, currentColorsList.Count > 0 ? palette.GetColor(currentColorsList[0]) : Color.clear);
        materialPropertyblock.SetColor(color02ID, currentColorsList.Count > 1 ? palette.GetColor(currentColorsList[1]) : Color.clear);
        materialPropertyblock.SetColor(color03ID, currentColorsList.Count > 2 ? palette.GetColor(currentColorsList[2]) : Color.clear);
        materialPropertyblock.SetColor(color04ID, currentColorsList.Count > 3 ? palette.GetColor(currentColorsList[3]) : Color.clear);

        bottleRenderer.SetPropertyBlock(materialPropertyblock);

        currentFillAmount = fill;

        UpdateBottleCompletionState();
    }
    void UpdateBottleCompletionState()
    {
        if (currentColorsList.Count != k_bottleCapacity)
        {
            isComplete = false;
            return;
        }

        int firstColor = currentColorsList[0];

        for (int i = 1; i < currentColorsList.Count; i++)
        {
            if (currentColorsList[i] != firstColor)
            {
                isComplete = false;
                return;
            }
        }

        isComplete = true;
        OnBottleComplete?.Invoke(this);
    }

    public int GetTopColorCount()
    {
        if (currentColorsList.Count == 0)
        {
            return 0;
        }

        int topColorIndex = currentColorsList[currentColorsList.Count - 1];
        int count = 0;

        for (int i = currentColorsList.Count - 1; i >= 0; i--)
        {
            if (currentColorsList[i] == topColorIndex)
            {
                count++;
            }
            else
            {
                break;
            }
        }

        return count;
    }

    public int GetTopColor()
    {
        if (currentColorsList.Count == 0)
        {
            return -1; // indicates no color
        }
        return currentColorsList[currentColorsList.Count - 1];
    }

    public bool CanReceiveThisColor(int colorIndex)
    {
        if (isComplete) return false;

        if (currentColorsList.Count == k_bottleCapacity)
        {
            return false;
        }
        if (currentColorsList.Count == 0)
        {
            return true;
        }
        return currentColorsList[currentColorsList.Count - 1] == colorIndex;
    }

    public void PourTo(Bottle target)
    {
        if (isAnimating)
        {
            OnPourComplete?.Invoke(this, target, false);
            return;
        }

        if (currentColorsList.Count == 0)
        {
            OnPourComplete?.Invoke(this, target, false);
            return;
        }

        if (isComplete || target.isComplete)
        {
            OnPourComplete?.Invoke(this, target, false);
            return;
        }

        int topColorIndex = GetTopColor();

        int sameColorCount = GetTopColorCount();
        int targetSpace = k_bottleCapacity - target.currentColorsList.Count;

        if (!target.CanReceiveThisColor(topColorIndex))
        {
            OnPourComplete?.Invoke(this, target, false);
            return;
        }

        // determining how much water need to be pour
        int amount = Mathf.Min(sameColorCount, targetSpace);
        if (amount <= 0)
        {
            OnPourComplete?.Invoke(this, target, false);
            return;
        }

        isAnimating = true;

        // rotate left or right (base on if target is left or right)
        float angle = GetPourAngleFromAmount(amount);

        // determine rotation direction based on target position
        float direction = target.bottleTransform.position.x < bottleTransform.position.x ? 1f : -1f;
        angle = angle * direction;

        float multiplierTarget = GetScaleAndRotationMultiplierFromAmount(amount);

        // prepare target color visually
        target.PrepareIncomingColor(topColorIndex, amount);

        // calculate target fill WITHOUT modifying list
        float sourceBottleStartFill = currentFillAmount;
        float sourceBottleTargetFill = (currentColorsList.Count - amount) / (float)k_bottleCapacity;

        float targetBottleStartFill = target.currentFillAmount;
        float targetBottleTargetFill = (target.currentColorsList.Count + amount) / (float)k_bottleCapacity;

        //Vector3 targetPourPosition = target.GetBottleMouthTransform().position;
        //targetPourPosition = new Vector3(targetPourPosition.x + (pourHorizontalOffset * direction), targetPourPosition.y, targetPourPosition.z);
        Vector3 targetPourPosition = new Vector3(target.bottleTransform.position.x + (pourHorizontalOffset * direction), target.bottleTransform.position.y + pourHeightOffset, bottleTransform.position.z);

        if (selectTween != null && selectTween.IsActive())
        {
            selectTween.Kill();
        }

        Sequence sequence = DOTween.Sequence();

        // move source bottle above target bottle
        sequence.Append(
            bottleTransform.DOMove(targetPourPosition, moveDuration)
                .SetEase(Ease.OutQuad)
        );

        // rotate bottle
        sequence.Append(
            bottleTransform
            .DORotate(new Vector3(0, 0, angle), pourDuration)
            .SetEase(Ease.OutQuad)
        );

        // increase multiplier
        sequence.Join(
            DOTween.To(
                () => scaleAndRotationMultiplier,
                x =>
                {
                    scaleAndRotationMultiplier = x;
                    UpdateShader();
                },
                multiplierTarget,
                pourDuration
            )
        );

        // drain source bottle
        sequence.Append(
            DOTween.To(
                () => currentFillAmount,
                x =>
                {
                    currentFillAmount = x;

                    bottleRenderer.GetPropertyBlock(materialPropertyblock);
                    materialPropertyblock.SetFloat(fillAmountID, currentFillAmount);
                    bottleRenderer.SetPropertyBlock(materialPropertyblock);
                },
                sourceBottleTargetFill,
                waterFillAndDrainDuration
            ).SetEase(Ease.InOutSine)
        );

        // fill target bottle
        sequence.Join(
            DOTween.To(
                () => target.currentFillAmount,
                x =>
                {
                    target.currentFillAmount = x;

                    target.bottleRenderer.GetPropertyBlock(target.materialPropertyblock);
                    target.materialPropertyblock.SetFloat(target.fillAmountID, target.currentFillAmount);
                    target.bottleRenderer.SetPropertyBlock(target.materialPropertyblock);
                },
                targetBottleTargetFill,
                waterFillAndDrainDuration
            ).SetEase(Ease.InOutSine)
        );

        // update shader colors after pour
        sequence.AppendCallback(() =>
        {
            for (int i = 0; i < amount; i++)
            {
                currentColorsList.RemoveAt(currentColorsList.Count - 1);
                target.currentColorsList.Add(topColorIndex);
            }

            UpdateShader();
            target.UpdateShader();
        });

        // move bottle back to orginal position
        sequence.Append(
             bottleTransform.DOMove(originalPosition, moveDuration)
                 .SetEase(Ease.InOutQuad)
        );

        // rotate bottle back
        sequence.Append(
            bottleTransform
            .DORotate(Vector3.zero, pourDuration)
            .SetEase(Ease.InOutQuad)
        );

        sequence.Join(
            DOTween.To(
                () => scaleAndRotationMultiplier,
                x =>
                {
                    scaleAndRotationMultiplier = x;
                    UpdateShader();
                },
                1f,
                pourDuration
            )
        );

        sequence.OnComplete(() =>
        {
            isAnimating = false;
            OnPourComplete?.Invoke(this, target, true);
        });
    }

    public void AnimateSelect()
    {
        if (selectTween != null && selectTween.IsActive())
            selectTween.Kill();

        selectTween = bottleTransform.DOLocalMoveY(
            originalLocalPosition.y + selectOffsetY,
            selectDuration
        ).SetEase(Ease.OutQuad);
    }

    public void AnimateDeselect()
    {
        if (selectTween != null && selectTween.IsActive())
            selectTween.Kill();

        selectTween = bottleTransform.DOLocalMoveY(
            originalLocalPosition.y,
            selectDuration
        ).SetEase(Ease.OutQuad);
    }

    void PrepareIncomingColor(int colorIndex, int amount)
    {
        Color color = palette.GetColor(colorIndex);

        bottleRenderer.GetPropertyBlock(materialPropertyblock);

        int startIndex = currentColorsList.Count;

        for (int i = 0; i < amount; i++)
        {
            int index = startIndex + i;

            if (index == 0) materialPropertyblock.SetColor(color01ID, color);
            else if (index == 1) materialPropertyblock.SetColor(color02ID, color);
            else if (index == 2) materialPropertyblock.SetColor(color03ID, color);
            else if (index == 3) materialPropertyblock.SetColor(color04ID, color);
        }

        bottleRenderer.SetPropertyBlock(materialPropertyblock);
    }

    public Tween AnimateReceiveFill(int amount)
    {
        float startFill = currentFillAmount;
        float targetFill = (currentColorsList.Count + amount) / (float)k_bottleCapacity;

        return DOTween.To(
            () => currentFillAmount,
            x =>
            {
                currentFillAmount = x;

                bottleRenderer.GetPropertyBlock(materialPropertyblock);
                materialPropertyblock.SetFloat(fillAmountID, currentFillAmount);
                bottleRenderer.SetPropertyBlock(materialPropertyblock);
            },
            targetFill,
            0.6f
        ).SetEase(Ease.InOutSine);
    }

    float GetPourAngleFromAmount(int amount)
    {
        switch (amount)
        {
            case 1: return 35f;
            case 2: return 65f;
            case 3: return 80f;
            default: return 95f;
        }
    }

    float GetScaleAndRotationMultiplierFromAmount(int amount)
    {
        switch (amount)
        {
            case 1: return 1f;
            case 2: return 1.5f;
            case 3: return 2f;
            default: return 3f;
        }
    }

    public string GetCurrentColorsInBottle()
    {
        string[] colors = new string[4];

        for (int i = 0; i < 4; i++)
        {
            if (i < currentColorsList.Count && currentColorsList[i] != null)
                colors[i] = currentColorsList[i].ToString();
            else
                colors[i] = "Empty";
        }

        return $"{colors[0]}, {colors[1]}, {colors[2]}, {colors[3]}";
    }
}