using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Hand : MonoBehaviour
{
    [SerializeField]
    List<Card> cards;
    [SerializeField]
    Vector3 positionShift;
    [SerializeField]
    float zPosiionShift;
    [SerializeField]
    int sortingTypeThreshold;
    [SerializeField]
    Vector3 fanSortingStartPosition;
    [SerializeField]
    float startAngle;
    [SerializeField]
    float endAngle;
    [SerializeField]
    ActiveCardController cardController;
    [SerializeField]
    BoardManager board;

    private Dictionary<Card, List<Tweener>> currentAnimations;
    private Card hoveringCard;
    
    int lenght;

    void Start()
    {
        currentAnimations = new Dictionary<Card, List<Tweener>>();

        Sort();

        for (int i = 0; i < lenght; i++)
        {
            cards[i].clickHandler.OnPick += OnCardPick;
            cards[i].clickHandler.OnPick += cardController.PickCard;
            cards[i].clickHandler.OnMouseEnterCallbacks += OnMouseEnterCardAnimation;
            cards[i].clickHandler.OnMouseLeaveCallbacks += OnMouseLeaveCardAnimation;
        }

        board.OnBoardSizeChange += OnBoardSizeChange;
    }

    private void OnCardPick(Card card) {
        KillCardTweens(card);
        card.cardDisplay.ResetTransform();
        hoveringCard = null;
        card.clickHandler.OnPick -= OnCardPick;
        card.clickHandler.OnPick -= cardController.PickCard;
        card.clickHandler.OnMouseEnterCallbacks -= OnMouseEnterCardAnimation;
        card.clickHandler.OnMouseLeaveCallbacks -= OnMouseLeaveCardAnimation;
    }

    private void KillCardTweens(Card card) {
        if (currentAnimations.ContainsKey(card))
        {
            foreach (Tweener tween1 in currentAnimations[card])
            {
                tween1.Kill();
            }
        }
    }

    private void OnMouseEnterCardAnimation(Card card) {
        if (hoveringCard == null) {
            hoveringCard = card;
            
            KillCardTweens(card);

            Vector3 scale = card.cardDisplay.mainObjectsTransform.localScale;
            card.cardDisplay.mainObjectsTransform.localScale = Vector3.one * 2;

            Vector3 rotation = card.transform.localRotation.eulerAngles;
            card.cardDisplay.mainObjectsTransform.localRotation = Quaternion.Euler(-rotation.x, -rotation.y, -rotation.z);

            float duration = 0.3f;
            Vector3 position = card.cardDisplay.mainObjectsTransform.position;
            position.y = -0.35f;
            card.cardDisplay.mainObjectsTransform.position = position;
            currentAnimations[card] = new List<Tweener>
            {
                card.cardDisplay.mainObjectsTransform.DOMoveY(-0.3f, duration).SetEase(Ease.OutQuad)
            };
            
            card.cardDisplay.SetRenderLayer("Active");
        }
    }

    private void OnMouseLeaveCardAnimation(Card card) {
        if (hoveringCard == card) {
            hoveringCard = null;
            KillCardTweens(card);
            card.cardDisplay.mainObjectsTransform.localScale = Vector3.one;
            Vector3 position = card.cardDisplay.mainObjectsTransform.localPosition;
            position.x = 0;
            position.y = 0.14f;
            card.cardDisplay.mainObjectsTransform.localPosition = position;

            float duration = 0.45f;

            currentAnimations[card] = new List<Tweener>
            {
                card.cardDisplay.mainObjectsTransform.DOLocalMove(new Vector3(), duration).SetEase(Ease.OutQuad),
                card.cardDisplay.mainObjectsTransform.DOLocalRotate(new Vector3(), duration).SetEase(Ease.OutQuad)
            };
            card.cardDisplay.SetRenderLayer("InHandCard" + (cards.Count - cards.IndexOf(card)));
        }
    }

    public void Sort() {
        lenght = cards.Count;
        if (lenght > sortingTypeThreshold)
            SortFan();
        else
            SortLinear();
    }

    public void SortLinear()
    {
        Vector3 startPosition = (-lenght / 2 + + 0.5f) * positionShift;
        if (lenght % 2 == 1)
            startPosition -= positionShift * 0.5f;

        for (int i = 0; i < lenght; i++)
        {
            cards[i].transform.localPosition = startPosition + positionShift * i;
            cards[i].cardDisplay.SetRenderLayer("InHandCard" + (lenght - i).ToString());
            Quaternion rotation = Quaternion.Euler(0, 0, 0);
            cards[i].transform.rotation = rotation;
        }
    }

    public void SortFan()
    {
        float fanSortingAngleShift = (endAngle - startAngle) / lenght;
        float localStartAngle = startAngle;

        for (int i = 0; i < lenght; i++)
        {
            float angle = localStartAngle + (fanSortingAngleShift * i);
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Quaternion cardRotation = Quaternion.Euler(0, 0, (startAngle + (fanSortingAngleShift * i)) * 0.5f);
            Vector3 position = rotation * fanSortingStartPosition - fanSortingStartPosition;
            position.z = 0.001f * i;
            cards[i].transform.localPosition = position;
            cards[i].transform.rotation = cardRotation;
            cards[i].cardDisplay.SetRenderLayer("InHandCard" + (lenght - i).ToString());
        }
    }
    public void Remove(Card card) {
        cards.Remove(card);
    }

    public void SetCardsClickable(bool active) {
        for (int i = 0; i < lenght; i++)
        {
            cards[i].clickHandler.SetClickable(active);
        }
    }    

    public void OnBoardSizeChange(int currentSize, int maxSize) {
        SetCardsClickable(!board.IsFilled);
    }

    Vector3 RotateTowardsUp(Vector3 start, float angle)
    {        
        return Quaternion.Euler(0, 0, angle) * start;
    }
}
