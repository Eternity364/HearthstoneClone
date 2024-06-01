using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using System.Text.RegularExpressions;

public class CardChangingStateAnimation : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer InHandImage;
    [SerializeField]
    SpriteRenderer InHandBezel;
    [SerializeField]
    CanvasGroup InHandTextGroup;
    [SerializeField]
    SpriteRenderer OnBoardImage;
    [SerializeField]
    SpriteRenderer OnBoardBezel;
    [SerializeField]
    CanvasGroup OnBoardTextGroup;


    public void Do(CardDisplay.DisplayStates toState)
    {
        SpriteRenderer fromImage = InHandImage;
        SpriteRenderer fromBezel = InHandBezel;
        CanvasGroup fromTextGroup = InHandTextGroup;
        SpriteRenderer toImage = OnBoardImage;
        SpriteRenderer toBezel = OnBoardBezel;
        CanvasGroup toTextGroup = OnBoardTextGroup;
        if (toState == CardDisplay.DisplayStates.InHand) {
            toImage = InHandImage;
            toBezel = InHandBezel;
            toTextGroup = InHandTextGroup;
            fromImage = OnBoardImage;
            fromBezel = OnBoardBezel;
            fromTextGroup = OnBoardTextGroup;
        }

        DOTween.To(AlphaSetter, 0, 1f, 1.3f).SetEase(Ease.OutQuad);

        void AlphaSetter(float alpha) {
            Color toColor = new Color(toImage.color.r, toImage.color.g, toImage.color.b, 255 * alpha);
            Color fromColor = new Color(toImage.color.r, toImage.color.g, toImage.color.b, 255 * (1 - alpha));
            fromImage.GetComponent<SpriteRenderer>().material.SetFloat("_Fade", (1 - alpha));
            fromBezel.GetComponent<SpriteRenderer>().material.SetFloat("_Fade", (1 - alpha));
            if (alpha == 1) {
                fromImage.color = fromColor;
                fromBezel.color = fromColor;
            }
            toImage.color = toColor;
            toBezel.color = toColor;
            toTextGroup.alpha = alpha;
            fromTextGroup.alpha = 1- alpha;
        }
    }
}

