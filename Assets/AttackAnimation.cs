using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AttackAnimation : MonoBehaviour
{

    public void DoPreparePart(Transform Transform, TweenCallback OnFinish)
    {
        float duration = 1f;
        Transform.DOLocalMoveZ(-0.5f, duration).SetEase(Ease.OutQuart).OnComplete(OnFinish);
    }

    public Sequence DoAttackPart(Transform transform, Vector3 target, TweenCallback OnFinish, TweenCallback OnFinishHit)
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOMove(target - new Vector3(0, 0.1f, 0), 0.25f).SetEase(Ease.InCubic).OnComplete(OnFinishHit));
        mySequence.Append(transform.DOLocalMove(Vector3.zero, 0.45f).SetEase(Ease.OutCubic).OnComplete(OnFinish));
        mySequence.Insert(0.25f, transform.DOLocalRotate(new Vector3(-12, 12, 0), 0.1f).SetEase(Ease.InCubic));
        mySequence.Insert(0.35f, transform.DOLocalRotate(new Vector3(), 0.35f).SetEase(Ease.InCubic));
        return mySequence;
    }

    // // Update is called once per frame
    // public void DoAttackPart(Transform cardTransform, Vector3 target, TweenCallback OnFinish)
    // {
    //     Sequence mySequencePosition = DOTween.Sequence();
    //     float duration = 0.6f;
    //     mySequencePosition.Append(cardTransform.DOLocalMove(new Vector3(-0.3f * cardTransform.localPosition.x, 0.1f, -1.2f), duration).SetEase(Ease.OutQuart).OnComplete(OnFirstPartFinish));
    //     mySequencePosition.Append(cardTransform.DOLocalMove(new Vector3(), duration).SetEase(Ease.InQuart));
    //     mySequencePosition.Insert(duration, mainObjectsTransform.DOScale(new Vector3(1.56f, 1.56f, 1), duration).SetEase(Ease.InQuart));
    //     mySequencePosition.OnComplete(OnFinish);
        
    //     Sequence mySequenceAngle = DOTween.Sequence();
    //     mySequenceAngle.Append(cardTransform.DOLocalRotate(new Vector3(), duration).SetEase(Ease.OutQuad));
    //     mySequenceAngle.Append(cardTransform.DOLocalRotate(new Vector3(-60, 22, 0), 0.5f).SetEase(Ease.InQuad));
    //     mySequenceAngle.Append(cardTransform.DOLocalRotate(new Vector3(), 0.1f).SetEase(Ease.OutQuad));
    // }
}

