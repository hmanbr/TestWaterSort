using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BottleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform bottle;
    [SerializeField] private Material liquidMaterial;

    [Header("Pour Settings")]
    [SerializeField] private float pourDuration = 0.4f;

    private int multiplierID;

    void Awake()
    {
        multiplierID = Shader.PropertyToID("_ScaleAndRotationMultiplier");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        void Update()
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
                Pour(35f);

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
                Pour(65f);

            if (Keyboard.current.digit3Key.wasPressedThisFrame)
                Pour(75f);

            if (Keyboard.current.digit4Key.wasPressedThisFrame)
                Pour(95f);
        }
    }

    void Pour(float angle)
    {
        float multiplier = Mathf.InverseLerp(0f, 95f, angle);

        Sequence seq = DOTween.Sequence();

        seq.Append(
            bottle.DORotate(
                new Vector3(0, 0, angle),
                pourDuration
            ).SetEase(Ease.OutQuad)
        );

        seq.Join(
            DOTween.To(
                () => liquidMaterial.GetFloat(multiplierID),
                x => liquidMaterial.SetFloat(multiplierID, x),
                multiplier,
                pourDuration
            )
        );

        seq.AppendInterval(0.5f);

        seq.Append(
            bottle.DORotate(Vector3.zero, pourDuration)
            .SetEase(Ease.InOutQuad)
        );

        seq.Join(
            DOTween.To(
                () => liquidMaterial.GetFloat(multiplierID),
                x => liquidMaterial.SetFloat(multiplierID, x),
                0f,
                pourDuration
            )
        );
    }
}
