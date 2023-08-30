using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardOnBoardPlacingAnimation : MonoBehaviour
{

    // Update is called once per frame
    public void Do(Transform cardTransform, Transform mainObjectsTransform, TweenCallback OnFirstPartFinish, TweenCallback OnFinish)
    {
        Sequence mySequencePosition = DOTween.Sequence();
        float duration = 0.6f;
        mySequencePosition.Append(cardTransform.DOLocalMove(new Vector3(-0.3f * cardTransform.localPosition.x, 0.1f, -1.2f), duration).SetEase(Ease.OutQuart).OnComplete(OnFirstPartFinish));
        mySequencePosition.Append(cardTransform.DOLocalMove(new Vector3(), duration).SetEase(Ease.InQuart));
        mySequencePosition.Insert(duration, mainObjectsTransform.DOScale(new Vector3(1.56f, 1.56f, 1), duration).SetEase(Ease.InQuart));
        mySequencePosition.OnComplete(OnFinish);
        
        Sequence mySequenceAngle = DOTween.Sequence();
        mySequenceAngle.Append(cardTransform.DOLocalRotate(new Vector3(), duration).SetEase(Ease.OutQuad));
        mySequenceAngle.Append(cardTransform.DOLocalRotate(new Vector3(-60, 22, 0), 0.5f).SetEase(Ease.InQuad));
        mySequenceAngle.Append(cardTransform.DOLocalRotate(new Vector3(), 0.1f).SetEase(Ease.OutQuad));
    }
}
