using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Unity.Netcode;

public class BoardManager : MonoBehaviour
{    
    [SerializeField]
    CardGenerator cardGenerator;
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
    Hand playerHand;
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
    public UnityAction<PlayerState, int, int> OnCardBattlecryBuff;
    private List<Card> enemyCardsOnBoard = new List<Card>();
    private List<Card> playerCardsOnBoard = new List<Card>();
    private List<Vector3> cardsPositions = new List<Vector3>();
    private List<Card> playerCardsOnBoardTemp = new List<Card>();
    private Queue<TweenCallback> attackAnimationQueue = new Queue<TweenCallback>();
    private List<Card> inactiveCards = new List<Card>();

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
    public bool InputLocked
    {
        get { return castingCard != null; }
    }
    public int TempIndex
    {
        get { return playerCardsOnBoardTemp.IndexOf(tempCard); }
    }

    private float positionShift = 0.4f;
    private Dictionary<List<Card>, List<Tweener>> sortingTweens;
    private Card attackingCard;
    private Card castingCard;
    private InputBlock handblock;

    public void Initialize () {
        sortingTweens = new Dictionary<List<Card>, List<Tweener>>();

        for (int i = 0; i < GameStateInstance.Instance.opponentCardsData.Count; i++)
        {
            PlaceCard(cardGenerator.Create(GameStateInstance.Instance.opponentCardsData[i].Index, enemyBoardTransform), PlayerState.Enemy, false);
        }
        for (int i = 0; i < GameStateInstance.Instance.playerCardsData.Count; i++)
        {
            PlaceCard(cardGenerator.Create(GameStateInstance.Instance.playerCardsData[i].Index, playerBoardTransform), PlayerState.Player, false);
        }

        SortCards(playerCardsOnBoard);
        SortCards(enemyCardsOnBoard);
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
        InputBlock block;
        block = InputBlockerInstace.Instance.AddCardBlock(card);

        void OnFirstPartFinish () {
           card.cardDisplay.SetPlacingParticlesActive(true);
        }

        void OnAnimationFinish () {
            card.cardDisplay.SetRenderLayer("Board");
            if (side == PlayerState.Player) {
                card.clickHandler.OnPick += OnCardClick;
            }
            else {
                card.clickHandler.OnMouseEnterCallbacks += OnCardMouseEnter;
                card.clickHandler.OnMouseLeaveCallbacks += OnCardMouseLeave;
                card.clickHandler.OnMouseUpEvents += AttemptToPerformAttack;
            }
            if (!(withAnimation && card.GetData().HasAbility(Ability.BattlecryBuff) && side == PlayerState.Player))
                InputBlockerInstace.Instance.RemoveBlock(block);
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
        card.cardDisplay.ChangeState(CardDisplay.DisplayStates.OnField, withAnimation);
    
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

            if (card.GetData().HasAbility(Ability.BattlecryBuff) && side == PlayerState.Player) {
                InputBlockerInstace.Instance.RemoveBlock(block);
                EnableBattlecryBuffMode(card);
            }
        }
        else
        {
            card.gameObject.transform.localPosition = Vector3.zero;
            card.cardDisplay.mainObjectsTransform.localScale = new Vector3(1.56f, 1.56f, 1f);
            cards.Add(card);
            OnAnimationFinish();
        }

        if (side == PlayerState.Player)
            OnBoardSizeChange.Invoke(cards.Count, maxBoardSize);
    }

    public void EnableBattlecryBuffMode(Card caster) {
        arrowController.SetActive(true, GetSortedPosition(caster, playerCardsOnBoard));
        castingCard = caster;
         
        playerHand.SetCardsClickable(false);      
        for (int i = 0; i < enemyCardsOnBoard.Count; i++)
        {
            enemyCardsOnBoard[i].clickHandler.SetClickable(false);
        }
        for (int i = 0; i < playerCardsOnBoard.Count; i++)
        {
            if (i != playerCardsOnBoard.IndexOf(caster)) {
                playerCardsOnBoard[i].clickHandler.OnPick -= OnCardClick;
                playerCardsOnBoard[i].clickHandler.OnPick += AttemptToPerformBattlecryBuff;
                playerCardsOnBoard[i].clickHandler.OnMouseEnterCallbacks += OnCardMouseEnter;
                playerCardsOnBoard[i].clickHandler.OnMouseLeaveCallbacks += OnCardMouseLeave;
            }
            playerCardsOnBoard[i].cardDisplay.SetActiveStatus(i != playerCardsOnBoard.IndexOf(caster));
            playerCardsOnBoard[i].clickHandler.SetClickable(true);
        }
    }

    public void DisableBattlecryBuffMode() {
        if (castingCard != null) {
            arrowController.SetActive(false, Vector2.zero);
            pointer.gameObject.SetActive(false);
            playerHand.SetCardsClickable(true);
            for (int i = 0; i < enemyCardsOnBoard.Count; i++)
            {
                enemyCardsOnBoard[i].clickHandler.SetClickable(true);
            }
            for (int i = 0; i < playerCardsOnBoard.Count; i++)
            {
                if (i != playerCardsOnBoard.IndexOf(castingCard)) {
                    playerCardsOnBoard[i].clickHandler.OnPick -= AttemptToPerformBattlecryBuff;
                    playerCardsOnBoard[i].clickHandler.OnPick += OnCardClick;
                    playerCardsOnBoard[i].clickHandler.OnMouseEnterCallbacks -= OnCardMouseEnter;
                    playerCardsOnBoard[i].clickHandler.OnMouseLeaveCallbacks -= OnCardMouseLeave;
                }
            }
            castingCard = null;
            handblock = null;
            InputBlockerInstace.Instance.UpdateValues(); 
        }
    }

    public void AttemptToPerformBattlecryBuff(Card target) {
        OnCardBattlecryBuff.Invoke(PlayerState.Player, playerCardsOnBoard.IndexOf(castingCard), playerCardsOnBoard.IndexOf(target));
        DisableBattlecryBuffMode();
    }

    public void PerformBattlecryBuff(PlayerState state, int casterIndex, int targetIndex) {
        Card caster, target;
        List<Card> cards;
        if (state == PlayerState.Player) {
            cards = playerCardsOnBoard;
        } 
        else 
        {
            cards = enemyCardsOnBoard;
        }
        caster = cards[casterIndex];
        target = cards[targetIndex];
        Buff buff = caster.GetData().battlecryBuff.buff;
        Buff copyBuff = new Buff(buff.health, buff.attack);
        target.cardDisplay.ApplyBuff(copyBuff);
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
        if (playerCardsOnBoardTemp == null) {
            playerCardsOnBoardTemp = new List<Card>();
            cardsPositions = new List<Vector3>();
            for (int i = 0; i < playerCardsOnBoard.Count; i++)
            {
                playerCardsOnBoardTemp.Add(playerCardsOnBoard[i]);
                cardsPositions.Add(playerCardsOnBoard[i].transform.localPosition);
                
            }
            playerCardsOnBoardTemp.Add(tempCard);
        }
    }

    public void StopTempSorting() {
        if (playerCardsOnBoardTemp != null) {
            playerCardsOnBoardTemp = null;
            SortCards(playerCardsOnBoard);
        }
    }

    public void SetCardActive(Card card, bool active) {
        card.clickHandler.SetClickable(active);
        if (playerCardsOnBoard.Contains(card))
            card.cardDisplay.SetActiveStatus(active);
    }

    public void SetCardsActive(bool active) {
        for (int i = 0; i < playerCardsOnBoard.Count; i++)
        {
            SetCardActive(playerCardsOnBoard[i], active);
        }
        for (int i = 0; i < enemyCardsOnBoard.Count; i++)
        {
            SetCardActive(enemyCardsOnBoard[i], active);
        }
    }    

    public void SortCards(List<Card> cards) {
        if (sortingTweens.ContainsKey(cards)) {
            for (int i = 0; i < sortingTweens[cards].Count; i++)
            {
                sortingTweens[cards][i].Kill();
            }
        }
        sortingTweens[cards] = new List<Tweener>();

        for (int i = 0; i < cards.Count; i++)
        {
            //cards[i].transform.localPosition = new Vector3(startPositionX + positionShift * i, cards[i].transform.localPosition.y,  cards[i].transform.localPosition.z);
            sortingTweens[cards].Add(cards[i].transform.DOMoveX(GetSortedPosition(cards[i], cards).x, 0.4f).SetEase(Ease.OutQuad));
        }
    }

    private Vector3 GetSortedPosition(Card card, List<Card> cards) {
        float startPositionX = (-cards.Count / 2 + 0.5f) * positionShift;
        if (cards.Count % 2 == 1)
            startPositionX -= positionShift * 0.5f;

        return new Vector3(startPositionX + positionShift * cards.IndexOf(card), card.transform.position.y, card.transform.position.z);
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

        
        Sequence mySequence = DOTween.Sequence();
        mySequence.InsertCallback(0.7f, Sort);

        void Sort () {
            if (deadTarget || deadAttacker) {
                SortCards(targetSet);
                SortCards(attackerSet);
            }
        }

        if (deadAttacker) {
            OnBoardSizeChange.Invoke(attackerSet.Count, maxBoardSize);
        }

        return deadAttacker;
    }

    private void OnCardClick(Card card) {
        arrowController.SetActive(true, card.transform.position);
        playerHand.SetCardsClickable(false);    
        attackingCard = card;
    }

    private void AttemptToPerformAttack(Card card) {
        if (attackingCard != null)
        {
            playerHand.SetCardsClickable(true);      
            OnCardAttack.Invoke(PlayerState.Player, playerCardsOnBoard.IndexOf(attackingCard), enemyCardsOnBoard.IndexOf(card));
        }
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
        void ActivateAttackParticle () {
            card.cardDisplay.SetAttackParticleActive();
        };
        void OnFinishPrepare () {
            card.cardDisplay.SetAttackParticleAngle(attackingCard1.transform.position);
            mySequence = attackAnimation.DoAttackPart(
                attackingCard1.cardDisplay.intermediateObjectsTransform,
                card.cardDisplay.intermediateObjectsTransform.position,
                OnFinishAttack,
                OnFinishHit,
                ActivateAttackParticle);
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
            if (playerCardsOnBoard.Contains(attackingCard1))
                InputBlockerInstace.Instance.AddCardBlock(attackingCard1);
            attackingCard1.cardDisplay.SetRenderLayer("Attacking");
            attackAnimation.DoPreparePart(attackingCard1.cardDisplay.intermediateObjectsTransform, AddToQueue);
            attackingCard = null;
        }
    }

    private void OnMouseButtonDrop() {
        if (castingCard == null) {
            if (!pointer.gameObject.activeSelf)
                attackingCard = null;
            arrowController.SetActive(false, Vector2.zero);
            pointer.gameObject.SetActive(false);
            playerHand.SetCardsClickable(true);   
        }
    }
    
    private void OnCardMouseEnter(Card card) {
        if (arrowController.Active)
            pointer.gameObject.SetActive(true);
    }

    private void OnCardMouseLeave(Card card) {
        pointer.gameObject.SetActive(false);
    }
}
