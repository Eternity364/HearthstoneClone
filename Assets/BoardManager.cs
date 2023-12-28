using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Unity.Netcode;

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
    [SerializeField]
    int maxBoardSize = 7;

    
    public UnityAction<int, int> OnBoardSizeChange;
    public UnityAction<bool, int, int> OnCardAttack;

    List<Card> placingCards = new List<Card>();
    List<Card> playerCardsOnBoard = new List<Card>();
    List<Vector3> cardsPositions = new List<Vector3>();
    List<Card> playerCardsOnBoardTemp = new List<Card>();
    Queue<TweenCallback> attackAnimationQueue = new Queue<TweenCallback>();

    public bool IsFilled
    {
        get { return playerCardsOnBoard.Count == maxBoardSize; }
    }

    float positionShift = 0.4f;
    Dictionary<List<Card>, List<Tweener>> sortingTweens;
    Card attackingCard;

    void Start () {
        sortingTweens = new Dictionary<List<Card>, List<Tweener>>();
        SortCards(enemyCardsOnBoard);
        for (int i = 0; i < enemyCardsOnBoard.Count; i++)
        {
            enemyCardsOnBoard[i].clickHandler.OnMouseEnterCallbacks += OnEnemyCardMouseEnter;
            enemyCardsOnBoard[i].clickHandler.OnMouseLeaveCallbacks += OnEnemyCardMouseLeave;
            enemyCardsOnBoard[i].clickHandler.OnMouseUpEvents += AttemptToPerformAttack;
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

        OnBoardSizeChange.Invoke(playerCardsOnBoard.Count, maxBoardSize);
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
        if (sortingTweens.ContainsKey(cards)) {
            for (int i = 0; i < sortingTweens[cards].Count; i++)
            {
                sortingTweens[cards][i].Kill();
            }
        }
        sortingTweens[cards] = new List<Tweener>();

        float startPositionX = (-cards.Count / 2 + 0.5f) * positionShift;
        if (cards.Count % 2 == 1)
            startPositionX -= positionShift * 0.5f;

        for (int i = 0; i < cards.Count; i++)
        {
            //cards[i].transform.localPosition = new Vector3(startPositionX + positionShift * i, cards[i].transform.localPosition.y,  cards[i].transform.localPosition.z);
            sortingTweens[cards].Add(cards[i].transform.DOMoveX(startPositionX + positionShift * i, 0.4f).SetEase(Ease.OutQuad));
        }
    }

    public void PerformAttackByIndex(bool attackerIsPlayer, int attackerIndex, int targetIndex) {
        Card attacker, target;
        if (attackerIsPlayer) {
            attacker = playerCardsOnBoard[attackerIndex];
            target = enemyCardsOnBoard[targetIndex];
        } 
        else 
        {
            attacker = enemyCardsOnBoard[attackerIndex];
            target = playerCardsOnBoard[targetIndex];
        }
        attackingCard = attacker;
        PerformAttack(target);
    }

    private bool FinishAttack(Card attacker, Card target) {
        bool deadTarget = target.DealDamage(attacker.cardDisplay.Data.Attack);
        bool deadAttacker = attacker.DealDamage(target.cardDisplay.Data.Attack);
        if (deadTarget) {
            enemyCardsOnBoard.Remove(target);
            SortCards(enemyCardsOnBoard);
        }
        if (deadAttacker) {
            playerCardsOnBoard.Remove(attacker);
            SortCards(playerCardsOnBoard);
            OnBoardSizeChange.Invoke(playerCardsOnBoard.Count, maxBoardSize);
        }
        return deadAttacker;
    }

    private void OnCardClick(Card card) {
        arrowController.SetActive(true, card.transform.position);
        attackingCard = card;
    }

    private void AttemptToPerformAttack(Card card) {
        if (attackingCard != null)
            OnCardAttack.Invoke(true, playerCardsOnBoard.IndexOf(attackingCard), enemyCardsOnBoard.IndexOf(card));
    }

    private void PerformAttack(Card card) {
        Card attackingCard1 = attackingCard;
        Sequence mySequence = null;
        
        void OnFinishHit () {
            bool dead = FinishAttack(attackingCard1, card);
            if (dead) {
                mySequence.Kill();
                if (attackAnimationQueue.Count >= 1) 
                    attackAnimationQueue.Dequeue()();
            }
        };
        void OnFinishAttack () {
            attackingCard1.clickHandler.SetClickable(true);
            if (attackAnimationQueue.Count >= 1) 
                attackAnimationQueue.Dequeue()();
        };
        void OnFinishPrepare () {
            mySequence = attackAnimation.DoAttackPart(
                attackingCard1.cardDisplay.intermediateObjectsTransform,
                card.cardDisplay.intermediateObjectsTransform.position,
                OnFinishAttack,
                OnFinishHit);
        };
        void Empty () {
            if (attackAnimationQueue.Count >= 1) 
                attackAnimationQueue.Dequeue()();
        };
        void AddToQueue () {
            attackAnimationQueue.Enqueue(OnFinishPrepare);
            if (attackAnimationQueue.Count == 1) {
                attackAnimationQueue.Dequeue()();
                attackAnimationQueue.Enqueue(Empty);
            }
        }
        if (attackingCard != null) {
            attackingCard1.clickHandler.SetClickable(false);
            attackAnimation.DoPreparePart(attackingCard1.cardDisplay.intermediateObjectsTransform, AddToQueue);
            if (attackingCard1.cardDisplay.Data.Attack >= card.cardDisplay.Data.Health) {
                card.clickHandler.SetClickable(false);
            }
            attackingCard = null;
        }
    }

    private void OnMouseButtonDrop() {
        if (!pointer.gameObject.activeSelf)
            attackingCard = null;
        arrowController.SetActive(false, Vector2.zero);
        pointer.gameObject.SetActive(false);
    }
    
    private void OnEnemyCardMouseEnter(Card card) {
        if (arrowController.Active)
            pointer.gameObject.SetActive(true);
    }

    private void OnEnemyCardMouseLeave(Card card) {
        pointer.gameObject.SetActive(false);
    }
}
