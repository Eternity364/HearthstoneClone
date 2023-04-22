using UnityEngine;

public class ActiveCardController : MonoBehaviour
{
    Card pickedCard;

    
    [SerializeField]
    public Hand hand;

    public void PickCard(Card card)
    {
        card.cardDisplay.gameObject.transform.SetParent(this.gameObject.transform);
        pickedCard = card;
    }

    private void DropPickedCard()
    {
        pickedCard.cardDisplay.gameObject.transform.SetParent(hand.gameObject.transform);
        pickedCard = null;
        hand.Sort();
    }

    
    void Update()
    {
        if (pickedCard != null) { 
            pickedCard.transform.localPosition = PositionGetter.GetPosition();
            if (Input.GetMouseButtonUp(0))
                DropPickedCard();
        }
    }
}
