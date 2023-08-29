using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardOnBoardPlacingAnimation : MonoBehaviour
{

    // Update is called once per frame
    public void Do(Transform transform)
    {
        Sequence mySequencePosition = DOTween.Sequence();
        mySequencePosition.Append(transform.DOMove(new Vector3(0, 0, -0.3f), 0.6f).SetEase(Ease.OutQuart));
        mySequencePosition.Append(transform.DOMove(new Vector3(0, 0, 0.96f), 0.6f).SetEase(Ease.InQuart));
        
        Sequence mySequenceAngle = DOTween.Sequence();
        mySequenceAngle.Append(transform.DORotate(new Vector3(), 0.6f).SetEase(Ease.OutQuad));
        mySequenceAngle.Append(transform.DORotate(new Vector3(-60, 22, 0), 0.5f).SetEase(Ease.InQuad));
        mySequenceAngle.Append(transform.DORotate(new Vector3(), 0.1f).SetEase(Ease.OutQuad));
    }
}
