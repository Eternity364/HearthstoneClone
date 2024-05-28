using UnityEngine;
using UnityEngine.Events;

public class ActiveCardController : MonoBehaviour
{
    Card pickedCard;

    
    [SerializeField]
    private Hand hand;
    [SerializeField]
    private BoardManager boardManager;

    public UnityAction<PlayerState, int, int> OnCardDrop;
    private int handIndex;
    

    public void PickCard(Card card, int handIndex)
    {
        card.cardDisplay.gameObject.transform.SetParent(this.gameObject.transform);
        card.cardDisplay.SetShadowActive(true);
        card.transform.rotation = Quaternion.Euler(0, 0, 0);
        pickedCard = card;
        boardManager.StartTempSorting();
        card.cardDisplay.SetRenderLayer("Active");
        hand.SetCardsClickable(false);
        this.handIndex = handIndex;
    }

    private void DropPickedCard()
    {        
        boardManager.PlaceCard(pickedCard, PlayerState.Player);
        pickedCard.RotationManager.SetActive(false);
        hand.Remove(pickedCard);
        hand.Sort();
        pickedCard = null;
        if (!boardManager.IsFilled)
            hand.SetCardsClickable(true);
        InputBlockerInstace.Instance.Update();

        OnCardDrop.Invoke(PlayerState.Player, handIndex, boardManager.TempIndex);
    }

    
    void Update()
    {
        if (pickedCard != null) {
            Vector3 position = PositionGetter.GetPosition(PositionGetter.ColliderType.ActiveCardController);
            if (position != Vector3.zero) {
                pickedCard.transform.localPosition = position;
                boardManager.ComparePositionsAndSortTemporarily(PositionGetter.GetPosition(PositionGetter.ColliderType.Background).x);
            }
            if (Input.GetMouseButtonUp(0))
                DropPickedCard();
        }
    }
}
