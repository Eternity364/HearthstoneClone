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
    [SerializeField]
    Card tempCard;
    List<Card> placingCards = new List<Card>();
    List<Card> cardsOnBoard = new List<Card>();
    List<Vector3> cardsPositions = new List<Vector3>();
    List<Card> cardsOnBoardTemp = new List<Card>();

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
        card.intermediateObjectsTransform.position = position;

        void OnFirstPartFinish () {
            card.cardDisplay.ChangeState(CardDisplay.DisplayStates.OnField);
        }

        void OnAnimationFinish () {
            card.cardDisplay.SetRenderLayer("Board");
        }
        placingAnimation.Do(card.intermediateObjectsTransform, card.mainObjectsTransform, OnFirstPartFinish, OnAnimationFinish);
        cardsOnBoard.Insert(cardsOnBoardTemp.IndexOf(tempCard), card);
        card.cardDisplay.SetRenderLayer("LandingOnBoard");
    
        SortCards(cardsOnBoard);
    }

    public void ComparePositionsAndSortTemporarily(float xPosition) {
        cardsOnBoardTemp.Remove(tempCard);
        int newIndex = 0;
        //print("newIndex = " + newIndex);
        print("cardsOnBoard.Count = " + cardsOnBoard.Count);
        print("cardsPositions.Count = " + cardsPositions.Count);
        for (int i = 0; i < cardsOnBoard.Count; i++)
        {
            if (xPosition < cardsPositions[i].x)
                break;
            else
                newIndex++;
            
        }
        cardsOnBoardTemp.Insert(newIndex, tempCard);
        SortCards(cardsOnBoardTemp);
    }

    public void StartTempSorting() {
        cardsOnBoardTemp = new List<Card>();
        cardsPositions = new List<Vector3>();
        for (int i = 0; i < cardsOnBoard.Count; i++)
        {
            cardsOnBoardTemp.Add(cardsOnBoard[i]);
            cardsPositions.Add(cardsOnBoard[i].transform.localPosition);
            
        }
        cardsOnBoardTemp.Add(tempCard);
    }

    public void SortCards(List<Card> cards) {
        for (int i = 0; i < sortingTweens.Count; i++)
        {
            sortingTweens[i].Kill();
        }
        sortingTweens = new List<Tweener>();

        float startPositionX = (-cards.Count / 2 + 0.5f) * positionShift;
        if (cards.Count % 2 == 1)
            startPositionX -= positionShift * 0.5f;

        for (int i = 0; i < cards.Count; i++)
        {
            //cards[i].transform.localPosition = new Vector3(startPositionX + positionShift * i, cards[i].transform.localPosition.y,  cards[i].transform.localPosition.z);
            sortingTweens.Add(cards[i].transform.DOMoveX(startPositionX + positionShift * i, 0.4f).SetEase(Ease.OutQuad));
        }
    }
}
