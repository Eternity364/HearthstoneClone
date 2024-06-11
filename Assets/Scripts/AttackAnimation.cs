using UnityEngine;
using DG.Tweening;

public class AttackAnimation : MonoBehaviour
{

    public void DoPreparePart(Transform Transform, TweenCallback OnFinish)
    {
        float duration = 1f;
        Transform.DOLocalMoveZ(-0.5f, duration).SetEase(Ease.OutQuart).OnComplete(OnFinish);
    }

    public Sequence DoAttackPart(Transform transform, Vector3 target, TweenCallback OnFinish, TweenCallback OnFinishHit, TweenCallback AttackParticle)
    {
        float duration = 0.25f;
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOMove(target - new Vector3(0, 0.1f, 0), 0.25f).SetEase(Ease.InCubic).OnComplete(OnFinishHit));
        mySequence.Append(transform.DOLocalMove(Vector3.zero, 0.45f).SetEase(Ease.OutCubic).OnComplete(OnFinish));
        mySequence.Insert(0.25f, transform.DOLocalRotate(new Vector3(-12, 12, 0), 0.1f).SetEase(Ease.InCubic));
        mySequence.Insert(0.35f, transform.DOLocalRotate(new Vector3(), 0.35f).SetEase(Ease.InCubic));
        mySequence.InsertCallback(duration - 0.07f, AttackParticle);
        return mySequence;
    }
}

