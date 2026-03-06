using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class BottlePourController : MonoBehaviour
{
    [SerializeField] private Transform bottle;
    [SerializeField] private Renderer bottleRenderer;

    [SerializeField] private float pourDuration = 0.4f;

    MaterialPropertyBlock propertyBlock;

    int multiplierID;
    int fillID;

    float currentFill = 1f;
    float currentMultiplier = 1f;

    const int maxLayers = 4;
    float layerHeight => 1f / maxLayers;

    bool isPouring;

    void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();

        multiplierID = Shader.PropertyToID("_ScaleAndRotationMultiplier");
        fillID = Shader.PropertyToID("_FillAmount");

        bottleRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(fillID, currentFill);
        propertyBlock.SetFloat(multiplierID, currentMultiplier);
        bottleRenderer.SetPropertyBlock(propertyBlock);
    }

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            Pour(35f, 1, 1f);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            Pour(65f, 2, 1.5f);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            Pour(80f, 3, 2f);

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            Pour(95f, 4, 3f);
    }

    void SetFill(float value)
    {
        currentFill = value;

        bottleRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(fillID, currentFill);
        bottleRenderer.SetPropertyBlock(propertyBlock);
    }

    void SetMultiplier(float value)
    {
        currentMultiplier = value;

        bottleRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(multiplierID, currentMultiplier);
        bottleRenderer.SetPropertyBlock(propertyBlock);
    }

    void Pour(float angle, int layersToRemove, float multiplierTarget)
    {
        if (isPouring) return;
        isPouring = true;

        float targetFill = Mathf.Clamp01(
            currentFill - (layersToRemove * layerHeight)
        );

        Sequence seq = DOTween.Sequence();

        seq.Append(
            bottle.DORotate(new Vector3(0, 0, angle), pourDuration)
            .SetEase(Ease.OutQuad)
        );

        seq.Join(
            DOTween.To(
                () => currentMultiplier,
                x => SetMultiplier(x),
                multiplierTarget,
                pourDuration
            )
        );

        seq.AppendInterval(0.2f);

        seq.Append(
            DOTween.To(
                () => currentFill,
                x => SetFill(x),
                targetFill,
                0.6f
            ).SetEase(Ease.InOutSine)
        );

        seq.Append(
            bottle.DORotate(Vector3.zero, pourDuration)
            .SetEase(Ease.InOutQuad)
        );

        seq.Join(
            DOTween.To(
                () => currentMultiplier,
                x => SetMultiplier(x),
                1f,
                pourDuration
            )
        );

        seq.OnComplete(() =>
        {
            isPouring = false;
        });
    }
}