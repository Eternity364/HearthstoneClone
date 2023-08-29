using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BoardManager : MonoBehaviour
{
    [SerializeField]
    CardOnBoardPlacingAnimation placingAnimation;
    [SerializeField]
    Transform playerBoardTransform;
    List<Card> placingCards = new List<Card>();
    List<Card> cardsOnBoard = new List<Card>();

    float positionShift = 0.4f;
    List<Tweener> sortingTweens;

    void Start () {
        sortingTweens = new List<Tweener>();
    }

    public void PlaceCard(Card card)
    {
        placingCards.Add(card);
        card.gameObject.transform.SetParent(playerBoardTransform);
        Vector3 position = card.transform.position;
        card.transform.localPosition = new Vector3();
        card.IntermediateParent.transform.position = position;

        placingAnimation.Do(card.IntermediateParent.transform, playerBoardTransform.position);
        cardsOnBoard.Add(card);
        SortCards();
    }

    public void SortCards() {
        for (int i = 0; i < sortingTweens.Count; i++)
        {
            sortingTweens[i].Kill();
        }
        sortingTweens = new List<Tweener>();

        float startPositionX = (-cardsOnBoard.Count / 2 + 0.5f) * positionShift;
        if (cardsOnBoard.Count % 2 == 1)
            startPositionX -= positionShift * 0.5f;

        for (int i = 0; i < cardsOnBoard.Count; i++)
        {
            //cardsOnBoard[i].transform.localPosition = new Vector3(startPositionX + positionShift * i, cardsOnBoard[i].transform.localPosition.y,  cardsOnBoard[i].transform.localPosition.z);
            //cardsOnBoard[i].cardDisplay.SetRenderOrder(i);
            sortingTweens.Add(cardsOnBoard[i].transform.DOMoveX(startPositionX + positionShift * i, 1f).SetEase(Ease.OutQuad));
        }
    }
}
