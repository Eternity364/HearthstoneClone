using UnityEngine;
using UnityEngine.Events;

public class InHandClickHandler : MonoBehaviour
{
    public UnityAction<Card> OnPick;

    [SerializeField]
    public Card card;

    private bool clicked = false;

    void OnMouseDown()
    {
        clicked = true;
        OnPick.Invoke(card);
        //print(card.cardDisplay.gameObject.name);
    }
}
