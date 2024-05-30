using System;
using UnityEngine;
using UnityEngine.Events;

public class ActiveCardController : MonoBehaviour
{
    Card pickedCard;

    
    [SerializeField]
    private Hand hand;
    [SerializeField]
    private BoardManager boardManager;
    [SerializeField]
    private Vector2 deadZone;

    public UnityAction<PlayerState, int, int> OnCardDrop;
    private int handIndex;
    private InputBlock handBlock;
    

    public void PickCard(Card card, int handIndex)
    {
        card.cardDisplay.gameObject.transform.SetParent(this.gameObject.transform);
        card.cardDisplay.SetShadowActive(true);
        card.transform.localRotation = Quaternion.Euler(0, 0, 0);
        pickedCard = card;
        boardManager.StartTempSorting();
        card.cardDisplay.SetRenderLayer("Active");
        handBlock = InputBlockerInstace.Instance.AddHandBlock();
        this.handIndex = handIndex;
    }

    public void ReturnCardToHand() {
        if (pickedCard != null) {
            pickedCard.RotationManager.SetActive(false);
            hand.ReturnCard(pickedCard, handIndex);
            pickedCard = null;
            InputBlockerInstace.Instance.RemoveBlock(handBlock);
            handBlock = null;
        }
    }

    private void DropPickedCard()
    {        
        boardManager.PlaceCard(pickedCard, PlayerState.Player);
        pickedCard.RotationManager.SetActive(false);
        hand.Remove(pickedCard);
        hand.Sort();
        pickedCard = null;
        InputBlockerInstace.Instance.RemoveBlock(handBlock);
        handBlock = null;

        OnCardDrop.Invoke(PlayerState.Player, handIndex, boardManager.TempIndex);
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
                else
                    DropPickedCard();
            } 
            else {
                if (IsPosInsideDeadZone(position))
                    boardManager.StopTempSorting();
                else if (position != Vector3.zero) {
                    boardManager.StartTempSorting();
                    boardManager.ComparePositionsAndSortTemporarily(PositionGetter.GetPosition(PositionGetter.ColliderType.Background).x);
                }
            }
        }
    }
}
