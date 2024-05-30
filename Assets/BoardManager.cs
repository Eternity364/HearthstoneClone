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
    Transform enemyBoardTransform;
    [SerializeField]
    Transform playerBoardTransform;
    [SerializeField]
    Card tempCard;
    [SerializeField]
    ArrowController arrowController;
    [SerializeField]
    public List<Card> enemyCardsSet;
    [SerializeField]
    public List<Card> playerCardsSet;
    [SerializeField]
    Transform pointer;
    [SerializeField]
    int maxBoardSize = 7;

    
    public UnityAction<int, int> OnBoardSizeChange;
    public UnityAction<PlayerState, int, int> OnCardAttack;
    private List<Card> enemyCardsOnBoard = new List<Card>();
    private List<Card> playerCardsOnBoard = new List<Card>();
    private List<Vector3> cardsPositions = new List<Vector3>();
    private List<Card> playerCardsOnBoardTemp = new List<Card>();
    private Queue<TweenCallback> attackAnimationQueue = new Queue<TweenCallback>();

    public bool IsFilled
    {
        get { return playerCardsOnBoard.Count == maxBoardSize; }
    }
    public List<Card> PlayerCardsOnBoard
    {
        get { return playerCardsOnBoard; }
    }
    public List<Card> EnemyCardsOnBoard
    {
        get { return enemyCardsOnBoard; }
    }
    public int TempIndex
    {
        get { return playerCardsOnBoardTemp.IndexOf(tempCard); }
    }

    private float positionShift = 0.4f;
    private Dictionary<List<Card>, List<Tweener>> sortingTweens;
    private Card attackingCard;

    public void Initialize (bool isPlayer) {
        sortingTweens = new Dictionary<List<Card>, List<Tweener>>();

        void AddEnemyCards (List<Card> cardsSet) {
            for (int i = 0; i < cardsSet.Count; i++)
            {
                enemyCardsOnBoard.Add(cardsSet[i]);
                cardsSet[i].gameObject.transform.SetParent(enemyBoardTransform);
                cardsSet[i].gameObject.transform.localPosition = Vector3.zero;
            }
        }
        void AddPlayerCards (List<Card> cardsSet) {
            for (int i = 0; i < cardsSet.Count; i++)
            {
                //cardsSet[i].gameObject.transform.localPosition = Vector3.zero;
                PlaceCard(cardsSet[i], PlayerState.Player, false);
            }
        }

        if (isPlayer) 
        {
            AddEnemyCards(enemyCardsSet);
            AddPlayerCards(playerCardsSet);
        } 
        else
        {
            AddEnemyCards(playerCardsSet);
            AddPlayerCards(enemyCardsSet);
        }

        SortCards(playerCardsOnBoard);
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

    public void SetInputActive(bool value)
    {
        for (int i = 0; i < playerCardsOnBoard.Count; i++)
        {
            playerCardsOnBoard[i].clickHandler.SetClickable(value);
        }
    }

    public void OnPlayerTurnStart()
    {
        for (int i = 0; i < playerCardsOnBoard.Count; i++)
        {
            InputBlockerInstace.Instance.RemoveCardBlock(playerCardsOnBoard[i]);
        }
        SetCardsStatusActive(true);
    }

    public void SetCardsStatusActive(bool value)
    {
        for (int i = 0; i < playerCardsOnBoard.Count; i++)
        {
            playerCardsOnBoard[i].cardDisplay.SetActiveStatus(value);
        }
    }

    public void DisableAttack() {
        arrowController.SetActive(false, Vector2.zero);
        attackingCard = null;
    }

    public void PlaceCard(Card card, PlayerState side, bool withAnimation = true, int forcedIndex = -1)
    {
        void OnFirstPartFinish () {
            card.cardDisplay.ChangeState(CardDisplay.DisplayStates.OnField);
        }

        void OnAnimationFinish () {
            card.cardDisplay.SetRenderLayer("Board");
            card.clickHandler.SetClickable(true);
            if (side == PlayerState.Player) {
                card.clickHandler.OnPick += OnCardClick;
                if (withAnimation)
                    InputBlockerInstace.Instance.AddCardBlock(card);
            }
            else {
                card.clickHandler.OnMouseEnterCallbacks += OnEnemyCardMouseEnter;
                card.clickHandler.OnMouseLeaveCallbacks += OnEnemyCardMouseLeave;
                card.clickHandler.OnMouseUpEvents += AttemptToPerformAttack;
            }
        }

        List<Card> cards = playerCardsOnBoard;
        if (side == PlayerState.Player) {
            card.gameObject.transform.SetParent(playerBoardTransform);
        }
        else {
            cards = enemyCardsOnBoard;
            card.gameObject.transform.SetParent(enemyBoardTransform);
        }
        Vector3 position = card.transform.position;
        card.transform.localPosition = new Vector3();
    
        if (withAnimation) 
        {
            card.cardDisplay.intermediateObjectsTransform.position = position;
            int index = forcedIndex;
            if (index == -1)
                index = playerCardsOnBoardTemp.IndexOf(tempCard);

            cards.Insert(index, card);
            card.cardDisplay.SetRenderLayer("LandingOnBoard");
            placingAnimation.Do(card.cardDisplay.intermediateObjectsTransform, card.cardDisplay.mainObjectsTransform, OnFirstPartFinish, OnAnimationFinish);
            SortCards(cards);
        }
        else
        {
            cards.Add(card);
            OnAnimationFinish();
        }

        if (side == PlayerState.Player)
            OnBoardSizeChange.Invoke(cards.Count, maxBoardSize);
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

    public void PerformAttackByCard(Card attacker, Card target) {
        attackingCard = attacker;
        PerformAttack(target);
    }

    public void OnCardDead(PlayerState state, int index) {
        if (state == PlayerState.Player) {
            InputBlockerInstace.Instance.RemoveCardBlock(playerCardsOnBoard[index]);
            playerCardsOnBoard[index].clickHandler.SetClickable(false);
            playerCardsOnBoard.RemoveAt(index);
            OnBoardSizeChange.Invoke(playerCardsOnBoard.Count, maxBoardSize);
        }
        if (state == PlayerState.Enemy) {
            enemyCardsOnBoard[index].clickHandler.SetClickable(false);
            enemyCardsOnBoard.RemoveAt(index);
        }
    }

    private bool FinishAttack(Card attacker, Card target) {
        bool deadTarget = target.DealDamage(attacker.GetData().Attack);
        bool deadAttacker = attacker.DealDamage(target.GetData().Attack);
        List<Card> attackerSet = playerCardsOnBoard;
        List<Card> targetSet = enemyCardsOnBoard;
        if (enemyCardsOnBoard.Contains(attacker)) {
            attackerSet = enemyCardsOnBoard;
            targetSet = playerCardsOnBoard;
        }

        if (deadTarget || deadAttacker) {
            SortCards(targetSet);
            SortCards(attackerSet);
        }

        if (deadAttacker) {
            OnBoardSizeChange.Invoke(attackerSet.Count, maxBoardSize);
        }

        return deadAttacker;
    }

    private void OnCardClick(Card card) {
        arrowController.SetActive(true, card.transform.position);
        attackingCard = card;
    }

    private void AttemptToPerformAttack(Card card) {
        if (attackingCard != null)
            OnCardAttack.Invoke(PlayerState.Player, playerCardsOnBoard.IndexOf(attackingCard), enemyCardsOnBoard.IndexOf(card));
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
            attackingCard1.cardDisplay.SetRenderLayer("Board");
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
            if (playerCardsOnBoard.Contains(attackingCard))
                InputBlockerInstace.Instance.AddCardBlock(attackingCard);
            attackingCard1.cardDisplay.SetRenderLayer("Attacking");
            attackAnimation.DoPreparePart(attackingCard1.cardDisplay.intermediateObjectsTransform, AddToQueue);
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
