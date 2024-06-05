using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Hand : MonoBehaviour
{
    [SerializeField]
    public List<Card> cards;
    [SerializeField]
    public CardGenerator cardGenerator;
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
    [SerializeField]
    PlayerState playerState;

    private Dictionary<Card, List<Tweener>> currentAnimationsMain = new Dictionary<Card, List<Tweener>>();
    private Dictionary<Card, List<Tweener>> currentAnimationsInside = new Dictionary<Card, List<Tweener>>();
    private Dictionary<Card, InputBlock> costBlocks = new Dictionary<Card, InputBlock>();
    private List<Card> inactiveCards = new List<Card>();
    private Card hoveringCard;
    private int takenCardIndex;
    private Card returningToHandCard;
    private InputBlock handBlock;

    public void Initialize(PlayerState state)
    {
        playerState = state;
        List<CardData> list = GameStateInstance.Instance.playerCardsInHandData;
        if (playerState == PlayerState.Enemy)
            list = GameStateInstance.Instance.opponentCardsInHandData;

        for (int i = 0; i < list.Count; i++)
        {
            AddCard(cardGenerator.Create(list[i].Index, transform));
        }

        Sort();

        if (playerState == PlayerState.Player) {
            board.OnBoardSizeChange += OnBoardSizeChange;
        }
    }
    
    public void AddCard(Card card) {
        cards.Add(card);
        if (playerState == PlayerState.Enemy) {
            card.cardDisplay.SetCardFrontActive(false);
            card.clickHandler.SetClickable(false);
        } 
        else
        {
            SetCardCallbacks(card, true);  
        }
    }

    public void DrawCard(Card card) {
        card.cardDisplay.SetRenderLayer("Active");
        float position2duration = 0.5f;
        float position3duration = 1f;
        float angle2time = position2duration;
        float angle2duration = position3duration;
        float scale1time = position2duration;
        float scale1duration = position3duration;
        Vector3 position1 = new Vector3(1.66f, -0.25f, 1.17f);
        Vector3 position2 = new Vector3(1.66f, -0.25f, 0.68f);
        if (playerState == PlayerState.Enemy) {
            position1.y = 0.5f;
            position2.y = 0.5f;
            angle2duration = 0.2f;
            card.cardDisplay.SetCardFrontActive(false);
        }
        Vector3 position3 = new Vector3(0.605f, -0.179f, 0.53f);
        Quaternion angle1 = Quaternion.Euler(4.288f, 105.292f, -87.655f);
        Vector3 angle2 = Vector3.zero;
        Vector2 scale1 = new Vector2(2, 2);
        Transform trans = card.transform;
        trans.position = position1;
        trans.rotation = angle1;
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(trans.DOMove(position2, position2duration).SetEase(Ease.OutCubic));
        if (playerState == PlayerState.Player) {
            mySequence.Append(trans.DOMove(position3, position3duration).SetEase(Ease.OutCubic));
            mySequence.Insert(scale1time, card.cardDisplay.mainObjectsTransform.DOScale(scale1, scale1duration)).SetEase(Ease.OutCubic);
        }
        mySequence.Insert(angle2time, trans.DOLocalRotate(angle2, angle2duration)).SetEase(Ease.OutCubic);
        mySequence.OnComplete(OnComplete);

        void OnComplete() {
            AddCard(card);
            Sort();
            if (cardController.pickedCard)
                cardController.SetInputBlock(true);
            InputBlockerInstace.Instance.UpdateValues();
        }
        // mySequence.Insert(0.25f, trans.DOLocalRotate(new Vector3(-12, 12, 0), 0.1f).SetEase(Ease.InCubic));
        // mySequence.Insert(0.35f, trans.DOLocalRotate(new Vector3(), 0.35f).SetEase(Ease.InCubic));
        // mySequence.InsertCallback(duration - 0.07f, AttackParticle);
    }

    private void OnCardPick(Card card) {
        KillCardTweens(card);
        card.cardDisplay.ResetTransform();
        SetCardCallbacks(card, false);
        takenCardIndex = cards.IndexOf(card);
        cards.Remove(card);
        cardController.PickCard(card, takenCardIndex);
        hoveringCard = null;
    }

    private void SetCardCallbacks(Card card, bool value) {
        if (value) {
            card.clickHandler.OnPick += OnCardPick;
            card.clickHandler.OnMouseEnterCallbacks += OnMouseEnterCardAnimation;
            card.clickHandler.OnMouseLeaveCallbacks += OnMouseLeaveCardAnimation;
        }
        else
        {
            card.clickHandler.OnPick -= OnCardPick;
            card.clickHandler.OnMouseEnterCallbacks -= OnMouseEnterCardAnimation;
            card.clickHandler.OnMouseLeaveCallbacks -= OnMouseLeaveCardAnimation;
        }
    }

    private void KillCardTweens(Card card) {
        KillCardMainTweens(card);
        KillCardInsideTweens(card);
    }

    private void KillCardMainTweens(Card card) {
        if (currentAnimationsMain.ContainsKey(card))
        {
            foreach (Tweener tween1 in currentAnimationsMain[card])
            {
                tween1.Kill();
            }
        }
    }

    private void KillCardInsideTweens(Card card) {
        if (currentAnimationsInside.ContainsKey(card))
        {
            foreach (Tweener tween1 in currentAnimationsInside[card])
            {
                tween1.Kill();
            }
        }
    }

    public void ReturnCard(Card card, int index) {
        cards.Insert(takenCardIndex, card);
        returningToHandCard = card;
        hoveringCard = null;
        card.cardDisplay.gameObject.transform.SetParent(gameObject.transform);
        card.cardDisplay.SetShadowActive(false);
        card.cardDisplay.ResetTransform();
        SetCardCallbacks(card, true);
        board.SortCards(board.PlayerCardsOnBoard);
        Sort();
        InputBlockerInstace.Instance.UpdateValues();
    }

    public void OnManaChange(PlayerState state, int currentMana, int mana) {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].cardDisplay.Data.Cost > currentMana) {
                if (!costBlocks.ContainsKey(cards[i]))
                    costBlocks[cards[i]] = InputBlockerInstace.Instance.AddCardBlock(cards[i]);
            } else {
                if (costBlocks.ContainsKey(cards[i])) {
                    InputBlockerInstace.Instance.RemoveCardBlock(cards[i]);
                    costBlocks.Remove(cards[i]);
                }
            }
        }
        
    }

    public void PlaceCard(Card card, int index) {
        Remove(card);
        board.PlaceCard(card, playerState, true, index);
        card.cardDisplay.SetCardFrontActive(true);
        Sort();
    }

    private void OnMouseEnterCardAnimation(Card card) {
        if (hoveringCard == null && !returningToHandCard == card) {
            hoveringCard = card;
            
            KillCardInsideTweens(card);
            
            SetCardSortingTransform(card, false);

            Vector3 scale = card.cardDisplay.mainObjectsTransform.localScale;
            card.cardDisplay.mainObjectsTransform.localScale = Vector3.one * 2;

            Vector3 rotation = card.transform.localRotation.eulerAngles;
            card.cardDisplay.mainObjectsTransform.localRotation = Quaternion.Euler(-rotation.x, -rotation.y, -rotation.z);

            float duration = 0.3f;
            Vector3 position = card.cardDisplay.mainObjectsTransform.position;
            position.y = -0.35f;
            card.cardDisplay.mainObjectsTransform.position = position;
            currentAnimationsInside[card] = new List<Tweener>
            {
                card.cardDisplay.mainObjectsTransform.DOMoveY(-0.3f, duration).SetEase(Ease.OutQuad)
            };
            
            card.cardDisplay.SetRenderLayer("Active");
        }
    }

    private void OnMouseLeaveCardAnimation(Card card) {
        if (hoveringCard == card) {
            hoveringCard = null;
            KillCardInsideTweens(card);
            card.cardDisplay.mainObjectsTransform.localScale = Vector3.one;
            Vector3 position = card.cardDisplay.mainObjectsTransform.localPosition;
            position.x = 0;
            position.y = 0.14f;
            card.cardDisplay.mainObjectsTransform.localPosition = position;

            float duration = 0.45f;

            currentAnimationsInside[card] = new List<Tweener>
            {
                card.cardDisplay.mainObjectsTransform.DOLocalMove(new Vector3(), duration).SetEase(Ease.OutQuad),
                card.cardDisplay.mainObjectsTransform.DOLocalRotate(new Vector3(), duration).SetEase(Ease.OutQuad)
            };
            card.cardDisplay.SetRenderLayer("InHandCard" + (cards.Count - cards.IndexOf(card)));
        }
    }

    public void Sort() {
        //if (cards.Count > sortingTypeThreshold)
            SortFan();
        // else
        //     SortLinear();
    }

    // public void SortLinear()
    // {
    //     Vector3 startPosition = (-cards.Count / 2 + + 0.5f) * positionShift;
    //     if (cards.Count % 2 == 1)
    //         startPosition -= positionShift * 0.5f;

    //     for (int i = 0; i < cards.Count; i++)
    //     {
    //         //cards[i].transform.localPosition = startPosition + positionShift * i;
    //         cards[i].cardDisplay.SetRenderLayer("InHandCard" + (cards.Count - i).ToString());
    //         // Quaternion rotation = Quaternion.Euler(0, 0, 0);
    //         // cards[i].transform.rotation = rotation;
            
    //         KillCardTweens(cards[i]);

    //         currentAnimations[cards[i]] = new List<Tweener>
    //         {
    //             cards[i].cardDisplay.mainObjectsTransform.DOLocalMove(startPosition + positionShift * i, 0.2f).SetEase(Ease.OutQuad),
    //             cards[i].cardDisplay.mainObjectsTransform.DORotate(new Vector3(), 0.2f).SetEase(Ease.OutQuad)
    //         };
    //     }
    // }

    public void SortFan()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            SetCardSortingTransform(cards[i]);
        }
    }

    private void SetCardSortingTransform(Card card, bool withAnimation = true) {
        KillCardMainTweens(card);
        float fanSortingAngleShift = (endAngle - startAngle) / cards.Count;
        float localStartAngle = startAngle;
        int i = cards.IndexOf(card);
        
        float angle = localStartAngle + (fanSortingAngleShift * i);
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        card.cardDisplay.SetRenderLayer("InHandCard" + (cards.Count - i).ToString());
        Vector3 position = rotation * fanSortingStartPosition - fanSortingStartPosition;
        position.z = 0.001f * i;
        Quaternion cardRotation = Quaternion.Euler(0, 0, (startAngle + (fanSortingAngleShift * i)) * 0.5f);

        void OnAnimationComplete() {
            if (card == returningToHandCard)
                returningToHandCard = null;
        }
        
        if (withAnimation) {
            currentAnimationsMain[cards[i]] = new List<Tweener>
            {
                card.transform.DOLocalMove(position, 0.2f).SetEase(Ease.OutQuad),
                card.transform.DOLocalRotateQuaternion(cardRotation, 0.2f).SetEase(Ease.OutQuad).OnKill(OnAnimationComplete),
                card.cardDisplay.mainObjectsTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad).OnKill(OnAnimationComplete)
            };
        } else {
            card.transform.localPosition = position;
            card.transform.localRotation = cardRotation;
            card.cardDisplay.mainObjectsTransform.localScale = Vector3.one;
        }
    }
    

    public void Remove(Card card) {
        cards.Remove(card);
    }

    public void SetCardActive(Card card, bool active) {
        if (active)
            inactiveCards.Remove(card);
        else if (!inactiveCards.Contains(card))  
            inactiveCards.Add(card);
        
        card.cardDisplay.SetActiveStatus(active);
    }     

    public void SetCardsActive(bool active) {
        for (int i = 0; i < cards.Count; i++)
        {
            SetCardActive(cards[i], active);
        }
    }    

    public void SetCardsClickable(bool active) {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].clickHandler.SetClickable(active);
        }
    }     

    public bool IsCardActive(Card card) {
        return !inactiveCards.Contains(card) && !costBlocks.ContainsKey(card);
    }    

    private void OnBoardSizeChange(int currentSize, int maxSize) {
        if (playerState == PlayerState.Player)
        {
            if (board.IsFilled) {
                if (handBlock == null)
                    handBlock = InputBlockerInstace.Instance.AddHandBlock();
            } else {
                if (handBlock != null) {
                    InputBlockerInstace.Instance.RemoveBlock(handBlock);
                    handBlock = null;
                }
            }
        }
    }

    Vector3 RotateTowardsUp(Vector3 start, float angle)
    {        
        return Quaternion.Euler(0, 0, angle) * start;
    }
}
