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

    private Dictionary<Card, Tweener> currentAnimations;
    private Card hoveringCard;
    
    int lenght;

    void Start()
    {
        currentAnimations = new Dictionary<Card, Tweener>();

        Sort();

        for (int i = 0; i < lenght; i++)
        {
            cards[i].clickHandler.OnPick += OnCardPick;
            cards[i].clickHandler.OnPick += cardController.PickCard;
            cards[i].clickHandler.OnMouseEnterCallbacks += OnMouseEnterCardAnimation;
            cards[i].clickHandler.OnMouseLeaveCallbacks += OnMouseLeaveCardAnimation;
        }
    }

    private void OnCardPick(Card card) {
        if (currentAnimations.ContainsKey(card))
            currentAnimations[card].Kill();
        card.cardDisplay.ResetTransform();
        hoveringCard = null;
    }

    private void OnMouseEnterCardAnimation(Card card) {
        if (hoveringCard == null) {
            hoveringCard = card;
            Vector3 scale = card.cardDisplay.mainObjectsTransform.localScale;
            card.cardDisplay.mainObjectsTransform.localScale = Vector3.one * 2;

            Vector3 rotation = card.transform.localRotation.eulerAngles;
            card.cardDisplay.mainObjectsTransform.localRotation = Quaternion.Euler(-rotation.x, -rotation.y, -rotation.z);

            float duration = 0.3f;
            Vector3 position = card.cardDisplay.mainObjectsTransform.position;
            position.y = -0.35f;
            card.cardDisplay.mainObjectsTransform.position = position;
            card.cardDisplay.mainObjectsTransform.DOMoveY(-0.3f, duration).SetEase(Ease.OutQuad);
            
            card.cardDisplay.SetRenderLayer("Active");
        }
    }

    private void OnMouseLeaveCardAnimation(Card card) {
        if (hoveringCard == card) {
            hoveringCard = null;
            card.cardDisplay.mainObjectsTransform.localScale = Vector3.one;
            card.cardDisplay.mainObjectsTransform.localRotation = Quaternion.Euler(Vector3.zero);

            float duration = 0.45f;
            card.cardDisplay.mainObjectsTransform.DOLocalMove(new Vector3(), duration).SetEase(Ease.OutQuad);
            
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
        Vector3 startPosition = (-lenght / 2 + 1) * positionShift;
        if (lenght % 2 == 1)
            startPosition -= positionShift;

        for (int i = 0; i < lenght; i++)
        {
            cards[i].transform.localPosition += startPosition + positionShift * i;
            cards[i].cardDisplay.SetRenderLayer("InHandCard" + i.ToString());
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

    Vector3 RotateTowardsUp(Vector3 start, float angle)
    {        
        return Quaternion.Euler(0, 0, angle) * start;
    }
}
