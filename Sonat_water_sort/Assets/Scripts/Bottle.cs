using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameEnums;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEditor.Experimental.GraphView.Port;
using static UnityEngine.Rendering.DebugUI;

public class Bottle : MonoBehaviour
{
    [SerializeField] Transform bottleTransform;
    [SerializeField] Renderer bottleRenderer;

    [SerializeField] ColorPaletteSO palette;
    [SerializeField] int[] setColors;

    [SerializeField] float pourDuration = 0.4f;

    const int bottleCapacity = 4;

    List<int> currentColorsList = new List<int>(bottleCapacity);

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

    void Awake()
    {
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
        InitializeFromPalette(setColors);
    }

    // Set bottle's color
    public void InitializeFromPalette(int[] setColors)
    {
        currentColorsList.Clear();

        foreach (var id in setColors)
        {
            if (id < 0) // empty slot
                continue;

            currentColorsList.Add(id);
        }

        UpdateShader();
    }

    void UpdateShader()
    {
        bottleRenderer.GetPropertyBlock(materialPropertyblock);

        float fill = currentColorsList.Count / (float)bottleCapacity;

        materialPropertyblock.SetFloat(fillAmountID, fill);
        materialPropertyblock.SetFloat(scaleAndRotationMultiplierID, scaleAndRotationMultiplier);

        materialPropertyblock.SetColor(color01ID, currentColorsList.Count > 0 ? palette.GetColor(currentColorsList[0]) : Color.clear);

        materialPropertyblock.SetColor(color02ID, currentColorsList.Count > 1 ? palette.GetColor(currentColorsList[1]) : Color.clear);

        materialPropertyblock.SetColor(color03ID, currentColorsList.Count > 2 ? palette.GetColor(currentColorsList[2]) : Color.clear);

        materialPropertyblock.SetColor(color04ID, currentColorsList.Count > 3 ? palette.GetColor(currentColorsList[3]) : Color.clear);

        bottleRenderer.SetPropertyBlock(materialPropertyblock);

        currentFillAmount = fill;
    }

    public int GetTopColorCount()
    {
        if (currentColorsList.Count == 0)
            return 0;

        int topColorIndex = currentColorsList[currentColorsList.Count - 1];
        int count = 0;

        for (int i = currentColorsList.Count - 1; i >= 0; i--)
        {
            if (currentColorsList[i] == topColorIndex)
                count++;
            else
                break;
        }

        return count;
    }

    public int GetTopColor()
    {
        if (currentColorsList.Count == 0)
            return -1; // indicates no color

        return currentColorsList[currentColorsList.Count - 1];
    }

    public bool CanReceiveThisColor(int colorIndex)
    {
        if (currentColorsList.Count == bottleCapacity)
            return false;

        if (currentColorsList.Count == 0)
            return true;

        return currentColorsList[currentColorsList.Count - 1] == colorIndex;
    }

    public void PourTo(Bottle target)
    {
        if (isAnimating) return;
        if (currentColorsList.Count == 0) return;

        int topColorIndex = GetTopColor();

        int sameColorCount = GetTopColorCount();
        int targetSpace = bottleCapacity - target.currentColorsList.Count;

        if (!target.CanReceiveThisColor(topColorIndex)) return;

        int amount = Mathf.Min(sameColorCount, targetSpace);
        if (amount <= 0) return;

        isAnimating = true;

        float angle = GetPourAngleFromAmount(amount);
        float multiplierTarget = GetScaleAndRotationMultiplierFromAmount(amount);

        // prepare target colors so animation shows correct color
        target.PrepareIncomingColor(topColorIndex, amount);

        // UPDATE LOGIC FIRST
        for (int i = 0; i < amount; i++)
        {
            currentColorsList.RemoveAt(currentColorsList.Count - 1);
            target.currentColorsList.Add(topColorIndex);
        }

        float sourceStartFill = currentFillAmount;
        float sourceTargetFill = currentColorsList.Count / (float)bottleCapacity;

        float targetStartFill = target.currentFillAmount;
        float targetTargetFill = target.currentColorsList.Count / (float)bottleCapacity;

        Sequence seq = DOTween.Sequence();

        // rotate bottle
        seq.Append(
            bottleTransform
            .DORotate(new Vector3(0, 0, angle), 0.35f)
            .SetEase(Ease.OutQuad)
        );

        // increase multiplier
        seq.Join(
            DOTween.To(
                () => scaleAndRotationMultiplier,
                x =>
                {
                    scaleAndRotationMultiplier = x;
                    UpdateShader();
                },
                multiplierTarget,
                0.35f
            )
        );

        // drain source bottle
        seq.Append(
            DOTween.To(
                () => currentFillAmount,
                x =>
                {
                    currentFillAmount = x;

                    bottleRenderer.GetPropertyBlock(materialPropertyblock);
                    materialPropertyblock.SetFloat(fillAmountID, currentFillAmount);
                    bottleRenderer.SetPropertyBlock(materialPropertyblock);
                },
                sourceTargetFill,
                0.6f
            ).SetEase(Ease.InOutSine)
        );

        // fill target bottle
        seq.Join(
            DOTween.To(
                () => target.currentFillAmount,
                x =>
                {
                    target.currentFillAmount = x;

                    target.bottleRenderer.GetPropertyBlock(target.materialPropertyblock);
                    target.materialPropertyblock.SetFloat(target.fillAmountID, target.currentFillAmount);
                    target.bottleRenderer.SetPropertyBlock(target.materialPropertyblock);
                },
                targetTargetFill,
                0.6f
            ).SetEase(Ease.InOutSine)
        );

        // update shader colors after pour
        seq.AppendCallback(() =>
        {
            UpdateShader();
            target.UpdateShader();
        });

        // rotate back
        seq.Append(
            bottleTransform
            .DORotate(Vector3.zero, 0.35f)
            .SetEase(Ease.InOutQuad)
        );

        seq.Join(
            DOTween.To(
                () => scaleAndRotationMultiplier,
                x =>
                {
                    scaleAndRotationMultiplier = x;
                    UpdateShader();
                },
                1f,
                0.35f
            )
        );

        seq.OnComplete(() =>
        {
            isAnimating = false;
        });
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
        float targetFill = (currentColorsList.Count + amount) / (float)bottleCapacity;

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