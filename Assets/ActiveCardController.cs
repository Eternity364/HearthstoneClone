using UnityEngine;

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
    }

    
    void Update()
    {
        if (pickedCard != null) {
            Vector3 position = PositionGetter.GetPosition();
            if (position != Vector3.zero)
                pickedCard.transform.localPosition = PositionGetter.GetPosition();
            if (Input.GetMouseButtonUp(0))
                DropPickedCard();
        }
    }
}
