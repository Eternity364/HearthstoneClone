using UnityEngine;
using DG.Tweening;

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
    [SerializeField]
    GameObject InHandOutline;
    [SerializeField]
    GameObject OnFieldOutline;


    public void Do(CardDisplay.DisplayStates toState, bool withAnimation = true)
    {
        SpriteRenderer fromImage = InHandImage;
        SpriteRenderer fromBezel = InHandBezel;
        CanvasGroup fromTextGroup = InHandTextGroup;
        GameObject fromOutline = InHandOutline;
        SpriteRenderer toImage = OnBoardImage;
        SpriteRenderer toBezel = OnBoardBezel;
        CanvasGroup toTextGroup = OnBoardTextGroup;
        GameObject toOutline = OnFieldOutline;
        if (toState == CardDisplay.DisplayStates.InHand) {
            toImage = InHandImage;
            toBezel = InHandBezel;
            toTextGroup = InHandTextGroup;
            toOutline = InHandOutline;
            fromImage = OnBoardImage;
            fromBezel = OnBoardBezel;
            fromTextGroup = OnBoardTextGroup;
            fromOutline = OnFieldOutline;
        }

        float duration = 1.3f;
        if (!withAnimation)
            duration = 0;
        DOTween.To(AlphaSetter, 0, 1f, duration).SetEase(Ease.OutQuad);
        fromOutline.SetActive(false);
        fromTextGroup.gameObject.SetActive(false);


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

