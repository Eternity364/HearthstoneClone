using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class DamageSplash : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer picture; 
    [SerializeField]
    Text text;  
    [SerializeField]
    CanvasGroup group;

    Sequence mySequence;
    
    public void Show(int damage)
    {
        print("SHowww");
        text.text = "-" + damage.ToString();
        gameObject.SetActive(true);
        if (mySequence != null)
            mySequence.Kill();
        AlphaSetter(1);
        transform.localScale = new Vector2(0.1f, 0.1f);
        group.gameObject.transform.localScale = new Vector2(0.1f, 0.1f);
        mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOScale(Vector2.one * 1.5f, 0.4f).SetEase(Ease.OutBounce));
        mySequence.Insert(0, group.gameObject.transform.DOScale(Vector2.one, 0.4f).SetEase(Ease.OutBounce));
        mySequence.AppendInterval(2f);
        mySequence.Append(DOTween.To(AlphaSetter, 1f, 0, 1f).SetEase(Ease.OutQuad));
        mySequence.OnComplete(Disable);
        
        void Disable() {
            gameObject.SetActive(false);
            mySequence = null;
        }

        void AlphaSetter(float alpha) {
            Color color = new Color(picture.color.r, picture.color.g, picture.color.b, alpha);
            picture.material.color = color;
            group.alpha = alpha;
        }
    }
}
