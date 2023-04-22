using UnityEngine;

public class ActiveCardController : MonoBehaviour
{
    Card pickedCard;

    public void PickCard(Card card)
    {
        card.cardDisplay.gameObject.transform.SetParent(this.gameObject.transform);
        pickedCard = card;
    }

    
    void Update()
    {
        if (pickedCard != null)
            pickedCard.transform.localPosition = PositionGetter.GetPosition();
    }
}
