using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardOnBoardPlacingAnimation : MonoBehaviour
{

    // Update is called once per frame
    public void Do(Transform cardTransform, TweenCallback OnFinish)
    {
        Sequence mySequencePosition = DOTween.Sequence();
        mySequencePosition.Append(cardTransform.DOLocalMove(new Vector3(-0.6f * cardTransform.localPosition.x, 0.1f, -1.2f), 0.6f).SetEase(Ease.OutQuart));
        mySequencePosition.Append(cardTransform.DOLocalMove(new Vector3(), 0.6f).SetEase(Ease.InQuart));
        mySequencePosition.OnComplete(OnFinish);
        
        Sequence mySequenceAngle = DOTween.Sequence();
        mySequenceAngle.Append(cardTransform.DOLocalRotate(new Vector3(), 0.6f).SetEase(Ease.OutQuad));
        mySequenceAngle.Append(cardTransform.DOLocalRotate(new Vector3(-60, 22, 0), 0.5f).SetEase(Ease.InQuad));
        mySequenceAngle.Append(cardTransform.DOLocalRotate(new Vector3(), 0.1f).SetEase(Ease.OutQuad));
    }
}
