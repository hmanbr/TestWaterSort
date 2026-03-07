using DG.Tweening;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DebugDotweenDoMove : MonoBehaviour
{
    [SerializeField] private Transform bottleTransform;
    [SerializeField] private float moveDuration;

    [SerializeField] private Transform targetToMoveTo;
    [SerializeField] private float pourHeightOffset;

    // JUUNK
    int fillAmountID;
    int scaleAndRotationMultiplierID;
    int color01ID;
    int color02ID;
    int color03ID;
    int color04ID;
    float currentFillAmount = 1f;
    float scaleAndRotationMultiplier = 1f;

    Vector3 originalPosition;


    private void Awake()
    {
        originalPosition = transform.position;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PourTo()
    {
        Vector3 targetPositionAbove = new Vector3(targetToMoveTo.transform.position.x, targetToMoveTo.transform.position.y + pourHeightOffset, targetToMoveTo.transform.position.z);
        Debug.Log("Target to move to: " + targetToMoveTo.transform.position);
        Debug.Log("Target above: " + targetPositionAbove);

        Sequence sequence = DOTween.Sequence();

        // move source bottle above target bottle
        sequence.Append(
            bottleTransform.DOMove(targetPositionAbove, moveDuration)
                .SetEase(Ease.OutQuad)
        );

        // increase multiplier
        sequence.Join(
            DOTween.To(
                () => scaleAndRotationMultiplier,
                x =>
                {
                    scaleAndRotationMultiplier = x;
                },
                3,
                0.35f
            )
        );

        // update shader colors after pour
        sequence.AppendCallback(() =>
        {
            for (int i = 0; i < 4; i++)
            {
                int a = i;
            }
        });

        // move bottle back to orginal position
        sequence.Append(
             bottleTransform.DOMove(originalPosition, moveDuration)
                 .SetEase(Ease.InOutQuad)
        );

        sequence.Join(
            DOTween.To(
                () => scaleAndRotationMultiplier,
                x =>
                {
                    scaleAndRotationMultiplier = x;
                },
                1f,
                0.35f
            )
        );

        sequence.OnComplete(() =>
        {

        });
    }
}
