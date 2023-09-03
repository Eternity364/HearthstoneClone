using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BoardManager : MonoBehaviour
{
    [SerializeField]
    CardOnBoardPlacingAnimation placingAnimation;
    [SerializeField]
    AttackAnimation attackAnimation;
    [SerializeField]
    Transform playerBoardTransform;
    [SerializeField]
    Card tempCard;
    [SerializeField]
    ArrowController arrowController;
    [SerializeField]
    List<Card> enemyCardsOnBoard = new List<Card>();
    [SerializeField]
    Transform pointer;

    List<Card> placingCards = new List<Card>();
    List<Card> playerCardsOnBoard = new List<Card>();
    List<Vector3> cardsPositions = new List<Vector3>();
    List<Card> playerCardsOnBoardTemp = new List<Card>();
    Queue<TweenCallback> attackAnimationQueue = new Queue<TweenCallback>();

    float positionShift = 0.4f;
    List<Tweener> sortingTweens;
    Card attackingCard;

    void Start () {
        sortingTweens = new List<Tweener>();
        SortCards(enemyCardsOnBoard);
        for (int i = 0; i < enemyCardsOnBoard.Count; i++)
        {
            enemyCardsOnBoard[i].clickHandler.OnMouseEnterCallbacks += OnEnemyCardMouseEnter;
            enemyCardsOnBoard[i].clickHandler.OnMouseLeaveCallbacks += OnEnemyCardMouseLeave;
            enemyCardsOnBoard[i].clickHandler.OnMouseUpEvents += OnEnemyCardMouseUp;
        }
    }

    void Update() {
        if (Input.GetMouseButtonUp(0))
            OnMouseButtonDrop();
        pointer.localPosition = PositionGetter.GetPosition(PositionGetter.ColliderType.Background);
    }

    public void PlaceCard(Card card)
    {
        placingCards.Add(card);
        card.gameObject.transform.SetParent(playerBoardTransform);
        Vector3 position = card.transform.position;
        card.transform.localPosition = new Vector3();
        card.cardDisplay.intermediateObjectsTransform.position = position;

        void OnFirstPartFinish () {
            card.cardDisplay.ChangeState(CardDisplay.DisplayStates.OnField);
        }

        void OnAnimationFinish () {
            card.cardDisplay.SetRenderLayer("Board");
            card.clickHandler.OnPick += OnCardClick;
            card.clickHandler.SetClickable(true);
        }
        placingAnimation.Do(card.cardDisplay.intermediateObjectsTransform, card.cardDisplay.mainObjectsTransform, OnFirstPartFinish, OnAnimationFinish);
        playerCardsOnBoard.Insert(playerCardsOnBoardTemp.IndexOf(tempCard), card);
        card.cardDisplay.SetRenderLayer("LandingOnBoard");
    
        SortCards(playerCardsOnBoard);
    }

    public void ComparePositionsAndSortTemporarily(float xPosition) {
        playerCardsOnBoardTemp.Remove(tempCard);
        int newIndex = 0;

        for (int i = 0; i < playerCardsOnBoard.Count; i++)
        {
            if (xPosition < cardsPositions[i].x)
                break;
            else
                newIndex++;
            
        }
        
        playerCardsOnBoardTemp.Insert(newIndex, tempCard);
        SortCards(playerCardsOnBoardTemp);
    }

    public void StartTempSorting() {
        playerCardsOnBoardTemp = new List<Card>();
        cardsPositions = new List<Vector3>();
        for (int i = 0; i < playerCardsOnBoard.Count; i++)
        {
            playerCardsOnBoardTemp.Add(playerCardsOnBoard[i]);
            cardsPositions.Add(playerCardsOnBoard[i].transform.localPosition);
            
        }
        playerCardsOnBoardTemp.Add(tempCard);
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

    private void OnCardClick(Card card) {
        arrowController.SetActive(true, card.transform.position);
        attackingCard = card;
    }

    private void OnEnemyCardMouseUp(Card card) {
        Card attackingCard1 = attackingCard;
        void OnFinishAttack () {
            attackingCard1.clickHandler.SetClickable(true);
            if (attackAnimationQueue.Count >= 1) 
                attackAnimationQueue.Dequeue()();
        };
        void OnFinishPrepare () {
            attackAnimation.DoAttackPart(attackingCard1.cardDisplay.intermediateObjectsTransform, card.cardDisplay.intermediateObjectsTransform.position, OnFinishAttack);
        };
        void Empty () {
            if (attackAnimationQueue.Count >= 1) 
                attackAnimationQueue.Dequeue()();
        };
        void AddToQueue () {
            attackAnimationQueue.Enqueue(OnFinishPrepare);
            print(attackAnimationQueue.Count);
            if (attackAnimationQueue.Count == 1) {
                attackAnimationQueue.Dequeue()();
                attackAnimationQueue.Enqueue(Empty);
            }
        }
        if (arrowController.Active) {
            attackingCard1.clickHandler.SetClickable(false);
            attackAnimation.DoPreparePart(attackingCard1.cardDisplay.intermediateObjectsTransform, AddToQueue);
        }
    }

    private void OnMouseButtonDrop() {
        arrowController.SetActive(false, Vector2.zero);
        pointer.gameObject.SetActive(false);
        attackingCard = null;
    }
    
    private void OnEnemyCardMouseEnter(Card card) {
        if (arrowController.Active)
            pointer.gameObject.SetActive(true);
    }

    private void OnEnemyCardMouseLeave(Card card) {
        pointer.gameObject.SetActive(false);
    }
}
