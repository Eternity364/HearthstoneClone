using UnityEngine;
using UnityEngine.UIElements;

public class ActiveCardController : MonoBehaviour
{
    Card pickedCard;

    
    [SerializeField]
    private Hand hand;
    [SerializeField]
    private BoardManager boardManager;

    public void PickCard(Card card)
    {
        card.cardDisplay.gameObject.transform.SetParent(this.gameObject.transform);
        card.cardDisplay.SetShadowActive(true);
        card.transform.rotation = Quaternion.Euler(0, 0, 0);
        pickedCard = card;
        boardManager.StartTempSorting();
        card.cardDisplay.SetRenderLayer("Active");
        hand.SetCardsClickable(false);
    }

    private void DropPickedCard()
    {
        //pickedCard.cardDisplay.gameObject.transform.SetParent(hand.gameObject.transform);
        //pickedCard.cardDisplay.SetShadowActive(false);
        
        boardManager.PlaceCard(pickedCard);
        pickedCard.RotationManager.SetActive(false);
        hand.Remove(pickedCard);
        hand.Sort();
        pickedCard = null;
        if (!boardManager.IsFilled)
            hand.SetCardsClickable(true);
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
