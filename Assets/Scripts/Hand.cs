using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Hand : MonoBehaviour
{
    [SerializeField]
    public List<Card> cards;
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

    private Dictionary<Card, List<Tweener>> currentAnimations;
    private Card hoveringCard;
    private Card returningToHandCard;
    private InputBlock handBlock;

    void Awake()
    {
        currentAnimations = new Dictionary<Card, List<Tweener>>();

        Sort();

        if (playerState == PlayerState.Player) {
            SetCardsCallbacks(true);
            board.OnBoardSizeChange += OnBoardSizeChange;
        } 
        else
        {
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].cardDisplay.SetCardFrontActive(false);
                cards[i].clickHandler.SetClickable(false);
            }

            //StartCoroutine(StartTestCardPlacing());
        }
    }

    IEnumerator StartTestCardPlacing()
    {
        yield return new WaitForSeconds(3);

        InputBlockerInstace.Instance.AddBlock();
    }

    private void OnCardPick(Card card) {
        KillCardTweens(card);
        card.cardDisplay.ResetTransform();
        hoveringCard = null;
        SetCardCallbacks(card, false);
        cardController.PickCard(card, cards.IndexOf(card));
    }

    private void SetCardsCallbacks(bool value) {
        for (int i = 0; i < cards.Count; i++)
        {
            SetCardCallbacks(cards[i], value);
        }
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
        if (currentAnimations.ContainsKey(card))
        {
            foreach (Tweener tween1 in currentAnimations[card])
            {
                tween1.Kill();
            }
        }
    }

    public void ReturnCard(Card card, int index) {
        cards[index] = card;
        returningToHandCard = card;
        Vector3 position = card.cardDisplay.gameObject.transform.position;
        card.cardDisplay.gameObject.transform.SetParent(gameObject.transform);
        card.cardDisplay.SetShadowActive(false);
        SetCardCallbacks(card, true);
        board.SortCards(board.PlayerCardsOnBoard);
        Sort();
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
            
            KillCardTweens(card);
            
            SetCardSortingTransform(card, false);

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
            print("OnLeave");
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
        //if (cards.Count > sortingTypeThreshold)
            SortFan();
        // else
        //     SortLinear();
    }

    public void SortLinear()
    {
        Vector3 startPosition = (-cards.Count / 2 + + 0.5f) * positionShift;
        if (cards.Count % 2 == 1)
            startPosition -= positionShift * 0.5f;

        for (int i = 0; i < cards.Count; i++)
        {
            //cards[i].transform.localPosition = startPosition + positionShift * i;
            cards[i].cardDisplay.SetRenderLayer("InHandCard" + (cards.Count - i).ToString());
            // Quaternion rotation = Quaternion.Euler(0, 0, 0);
            // cards[i].transform.rotation = rotation;
            
            KillCardTweens(cards[i]);

            currentAnimations[cards[i]] = new List<Tweener>
            {
                cards[i].cardDisplay.mainObjectsTransform.DOLocalMove(startPosition + positionShift * i, 0.2f).SetEase(Ease.OutQuad),
                cards[i].cardDisplay.mainObjectsTransform.DORotate(new Vector3(), 0.2f).SetEase(Ease.OutQuad)
            };
        }
    }

    public void SortFan()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            SetCardSortingTransform(cards[i]);
        }
        print("Yep");
    }

    private void SetCardSortingTransform(Card card, bool withAnimation = true) {
        KillCardTweens(card);
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
            currentAnimations[cards[i]] = new List<Tweener>
            {
                card.transform.DOLocalMove(position, 0.2f).SetEase(Ease.OutQuad),
                card.transform.DOLocalRotateQuaternion(cardRotation, 0.2f).SetEase(Ease.OutQuad).OnKill(OnAnimationComplete)
            };
        } else {
            card.transform.localPosition = position;
            card.transform.localRotation = cardRotation;
        }
    }
    

    public void Remove(Card card) {
        cards.Remove(card);
    }

    public void SetCardsClickable(bool active) {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].clickHandler.SetClickable(active);
        }
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

    
    public Sequence StartSortingAnimation(Transform transform, Vector3 target, TweenCallback OnFinish, TweenCallback OnFinishHit)
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOMove(target - new Vector3(0, 0.1f, 0), 0.25f).SetEase(Ease.InCubic).OnComplete(OnFinishHit));
        mySequence.Append(transform.DOLocalMove(Vector3.zero, 0.45f).SetEase(Ease.OutCubic).OnComplete(OnFinish));
        mySequence.Insert(0.25f, transform.DOLocalRotate(new Vector3(-12, 12, 0), 0.1f).SetEase(Ease.InCubic));
        mySequence.Insert(0.35f, transform.DOLocalRotate(new Vector3(), 0.35f).SetEase(Ease.InCubic));
        return mySequence;
    }

    Vector3 RotateTowardsUp(Vector3 start, float angle)
    {        
        return Quaternion.Euler(0, 0, angle) * start;
    }
}
