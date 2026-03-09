using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Bottle : MonoBehaviour
{
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

    Material bottleMaterial;

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

    [SerializeField] float selectOffsetY = 0.4f;
    [SerializeField] float selectDuration = 0.2f;

    Vector3 originalLocalPosition;
    Tween selectTween;

    [SerializeField] float pourHeightOffset = 5f;
    [SerializeField] float pourHorizontalOffset = 0.35f;
    [SerializeField] float moveDuration = 0.25f;

    Vector3 originalPosition;

    [SerializeField] Transform bottleMouthTransform;

    public Transform GetBottleMouthTransform()
    {
        return bottleMouthTransform;
    }

    void Awake()
    {
        originalLocalPosition = bottleTransform.localPosition;
        originalPosition = bottleTransform.position;

        bottleMaterial = bottleRenderer.material;

        fillAmountID = Shader.PropertyToID("_FillAmount");
        scaleAndRotationMultiplierID = Shader.PropertyToID("_ScaleAndRotationMultiplier");

        color01ID = Shader.PropertyToID("_Color01");
        color02ID = Shader.PropertyToID("_Color02");
        color03ID = Shader.PropertyToID("_Color03");
        color04ID = Shader.PropertyToID("_Color04");

        UpdateShader();
    }

    void Start()
    {
        InitializeFromPalette(setColors);
    }

    public void InitializeFromPalette(int[] setColors)
    {
        currentColorsList.Clear();

        foreach (var id in setColors)
        {
            if (id < 0) continue;
            currentColorsList.Add(id);
        }

        UpdateShader();
    }

    void UpdateShader()
    {
        float fill = currentColorsList.Count / (float)k_bottleCapacity;

        bottleMaterial.SetFloat(fillAmountID, fill);
        bottleMaterial.SetFloat(scaleAndRotationMultiplierID, scaleAndRotationMultiplier);

        bottleMaterial.SetColor(color01ID, currentColorsList.Count > 0 ? palette.GetColor(currentColorsList[0]) : Color.clear);
        bottleMaterial.SetColor(color02ID, currentColorsList.Count > 1 ? palette.GetColor(currentColorsList[1]) : Color.clear);
        bottleMaterial.SetColor(color03ID, currentColorsList.Count > 2 ? palette.GetColor(currentColorsList[2]) : Color.clear);
        bottleMaterial.SetColor(color04ID, currentColorsList.Count > 3 ? palette.GetColor(currentColorsList[3]) : Color.clear);

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

    public int GetTopColor()
    {
        if (currentColorsList.Count == 0)
            return -1;

        return currentColorsList[currentColorsList.Count - 1];
    }

    public int GetTopColorCount()
    {
        if (currentColorsList.Count == 0)
            return 0;

        int topColor = GetTopColor();
        int count = 0;

        for (int i = currentColorsList.Count - 1; i >= 0; i--)
        {
            if (currentColorsList[i] == topColor)
                count++;
            else
                break;
        }

        return count;
    }

    public bool CanReceiveThisColor(int colorIndex)
    {
        if (isComplete) return false;
        if (currentColorsList.Count == k_bottleCapacity) return false;

        if (currentColorsList.Count == 0)
            return true;

        return currentColorsList[currentColorsList.Count - 1] == colorIndex;
    }

    public bool CanBeSourceBottle()
    {
        if (currentColorsList.Count == 0) return false;
        if (isComplete) return false;
        return true;
    }

    public void PourTo(Bottle target)
    {
        if (isAnimating)
        {
            FailPourAttempt(target);
            return;
        }

        if (currentColorsList.Count == 0)
        {
            FailPourAttempt(target);
            return;
        }

        if (isComplete || target.isComplete)
        {
            FailPourAttempt(target);
            return;
        }

        int topColor = GetTopColor();

        int sameColorCount = GetTopColorCount();
        int targetSpace = k_bottleCapacity - target.currentColorsList.Count;

        if (!target.CanReceiveThisColor(topColor))
        {
            FailPourAttempt(target);
            return;
        }

        int amount = Mathf.Min(sameColorCount, targetSpace);
        if (amount <= 0)
        {
            FailPourAttempt(target);
            return;
        }

        isAnimating = true;

        float angle = GetPourAngleFromAmount(amount);
        float direction = target.bottleTransform.position.x < bottleTransform.position.x ? 1f : -1f;
        angle *= direction;

        float multiplierTarget = GetScaleAndRotationMultiplierFromAmount(amount);

        target.PrepareIncomingColor(topColor, amount);

        float sourceTargetFill = (currentColorsList.Count - amount) / (float)k_bottleCapacity;
        float targetTargetFill = (target.currentColorsList.Count + amount) / (float)k_bottleCapacity;

        Vector3 targetPourPosition =
            new Vector3(
                target.bottleTransform.position.x + (pourHorizontalOffset * direction),
                target.bottleTransform.position.y + pourHeightOffset,
                bottleTransform.position.z
            );

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            bottleTransform.DOMove(targetPourPosition, moveDuration)
        );

        sequence.Append(
            bottleTransform.DORotate(new Vector3(0, 0, angle), pourDuration)
        );

        sequence.Join(
            DOTween.To(
                () => scaleAndRotationMultiplier,
                x =>
                {
                    scaleAndRotationMultiplier = x;
                    bottleMaterial.SetFloat(scaleAndRotationMultiplierID, x);
                },
                multiplierTarget,
                pourDuration
            )
        );

        sequence.Append(
            DOTween.To(
                () => currentFillAmount,
                x =>
                {
                    currentFillAmount = x;
                    bottleMaterial.SetFloat(fillAmountID, x);
                },
                sourceTargetFill,
                waterFillAndDrainDuration
            )
        );

        sequence.Join(
            DOTween.To(
                () => target.currentFillAmount,
                x =>
                {
                    target.currentFillAmount = x;
                    target.bottleMaterial.SetFloat(target.fillAmountID, x);
                },
                targetTargetFill,
                waterFillAndDrainDuration
            )
        );

        sequence.AppendCallback(() =>
        {
            for (int i = 0; i < amount; i++)
            {
                currentColorsList.RemoveAt(currentColorsList.Count - 1);
                target.currentColorsList.Add(topColor);
            }

            UpdateShader();
            target.UpdateShader();
        });

        sequence.Append(
            bottleTransform.DOMove(originalPosition, moveDuration)
        );

        sequence.Append(
            bottleTransform.DORotate(Vector3.zero, pourDuration)
        );

        sequence.Join(
            DOTween.To(
                () => scaleAndRotationMultiplier,
                x =>
                {
                    scaleAndRotationMultiplier = x;
                    bottleMaterial.SetFloat(scaleAndRotationMultiplierID, x);
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

    void PrepareIncomingColor(int colorIndex, int amount)
    {
        Color color = palette.GetColor(colorIndex);
        int startIndex = currentColorsList.Count;

        for (int i = 0; i < amount; i++)
        {
            int index = startIndex + i;

            if (index == 0) bottleMaterial.SetColor(color01ID, color);
            else if (index == 1) bottleMaterial.SetColor(color02ID, color);
            else if (index == 2) bottleMaterial.SetColor(color03ID, color);
            else if (index == 3) bottleMaterial.SetColor(color04ID, color);
        }
    }
    public Tween AnimateReceiveFill(int amount)
    {
        float targetFill = (currentColorsList.Count + amount) / (float)k_bottleCapacity;

        return DOTween.To(
            () => currentFillAmount,
            x =>
            {
                currentFillAmount = x;
                bottleMaterial.SetFloat(fillAmountID, currentFillAmount);
            },
            targetFill,
            0.6f
        ).SetEase(Ease.InOutSine);
    }

    void FailPourAttempt(Bottle target)
    {
        OnPourComplete?.Invoke(this, target, false);
        AnimateDeselect();
    }

    public void AnimateSelect()
    {
        if (selectTween != null && selectTween.IsActive())
            selectTween.Kill();

        selectTween = bottleTransform.DOLocalMoveY(
            originalLocalPosition.y + selectOffsetY,
            selectDuration
        );
    }

    public void AnimateDeselect()
    {
        if (selectTween != null && selectTween.IsActive())
            selectTween.Kill();

        selectTween = bottleTransform.DOLocalMoveY(
            originalLocalPosition.y,
            selectDuration
        );
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
            if (i < currentColorsList.Count)
                colors[i] = currentColorsList[i].ToString();
            else
                colors[i] = "Empty";
        }

        return $"{colors[0]}, {colors[1]}, {colors[2]}, {colors[3]}";
    }
}