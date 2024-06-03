using System;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections.Generic;

public class ActiveCardController : MonoBehaviour
{

    
    [SerializeField]
    private Hand hand;
    [SerializeField]
    private BoardManager boardManager;
    [SerializeField]
    private Vector2 deadZone;

    public UnityAction<PlayerState, int, int> OnCardDrop;
    public Card pickedCard;

    private int handIndex;
    private List<InputBlock> handBlocks = new List<InputBlock>();
    

    public void PickCard(Card card, int handIndex)
    {
        SetInputBlock(true);
        card.cardDisplay.gameObject.transform.SetParent(this.gameObject.transform);
        card.cardDisplay.SetShadowActive(true);
        card.transform.localRotation = Quaternion.Euler(0, 0, 0);
        pickedCard = card;
        boardManager.StartTempSorting();
        card.cardDisplay.SetRenderLayer("Active");
        card.cardDisplay.SetPickedCardParticlesActive(true);
        this.handIndex = handIndex;
    }

    public void ReturnCardToHand() {
        if (pickedCard != null) {
            pickedCard.RotationManager.SetActive(false);
            pickedCard.cardDisplay.SetPickedCardParticlesActive(false);
            hand.ReturnCard(pickedCard, handIndex);
            pickedCard = null;
            SetInputBlock(false);
        }
    }

    private void DropPickedCard()
    {        
        pickedCard.cardDisplay.SetActiveStatus(false);
        boardManager.PlaceCard(pickedCard, PlayerState.Player);
        pickedCard.RotationManager.SetActive(false);
        pickedCard.cardDisplay.SetPickedCardParticlesActive(false);
        hand.Sort();
        pickedCard = null;
        SetInputBlock(false);

        OnCardDrop.Invoke(PlayerState.Player, handIndex, boardManager.TempIndex);
    }

    private void SetInputBlock(bool value)
    {      
        if (value) {  
            for (int i = 0; i < hand.cards.Count; i++)
            {
                handBlocks.Add(InputBlockerInstace.Instance.AddCardBlock(hand.cards[i]));
            }
        } else
        {
            for (int i = 0; i < handBlocks.Count; i++)
            {
                InputBlockerInstace.Instance.RemoveBlock(handBlocks[i]);
            }
            handBlocks = new List<InputBlock>();
        }
    }

    private bool IsPosInsideDeadZone(Vector2 position) {
        return Math.Abs(position.x) < deadZone.x && position.y < deadZone.y;
    }
    
    void Update()
    {
        if (pickedCard != null) {
            Vector3 position = PositionGetter.GetPosition(PositionGetter.ColliderType.ActiveCardController);   
            pickedCard.transform.localPosition = position; 
            if (Input.GetMouseButtonUp(0)) {
                if (IsPosInsideDeadZone(position)) {
                    ReturnCardToHand();
                }
                else if (hand.IsCardActive(pickedCard))
                    DropPickedCard();
            } 
            else {
                if (IsPosInsideDeadZone(position))
                    boardManager.StopTempSorting();
                else if (position != Vector3.zero) {
                    if (hand.IsCardActive(pickedCard)) {
                        boardManager.StartTempSorting();
                        boardManager.ComparePositionsAndSortTemporarily(PositionGetter.GetPosition(PositionGetter.ColliderType.Background).x);
                    }
                    else {
                        ReturnCardToHand();
                    }
                }
            }
        }
    }
}
