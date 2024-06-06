using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DivineShield : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer rend;
    
    public void Appear()
    {
        gameObject.SetActive(true);
        float duration = 0.5f;
        transform.DOScale(new Vector3(0.064f, 0.064f, 1), duration).SetEase(Ease.OutBounce);
        DOTween.To(AlphaSetter, 0, 1f, duration).SetEase(Ease.OutQuad);
        void AlphaSetter(float alpha) {
            Color color = new Color(rend.color.r, rend.color.g, rend.color.b, 255 * alpha);
            rend.color = color;
        }
    }

    public void Disappear()
    {
        float duration = 0.5f;
        transform.DOScale(new Vector3(0.1f, 0.1f, 1), duration).SetEase(Ease.InBounce);
        DOTween.To(AlphaSetter, 1, 0f, duration).SetEase(Ease.OutQuad);
        void AlphaSetter(float alpha) {
            Color color = new Color(rend.color.r, rend.color.g, rend.color.b, 255 * alpha);
            rend.color = color;
        }
    }
}
