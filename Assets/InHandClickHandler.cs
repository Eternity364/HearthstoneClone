using UnityEngine;
using UnityEngine.Events;

public class InHandClickHandler : MonoBehaviour
{
    public UnityAction<Card> OnPick;

    [SerializeField]
    public Card card;
    [SerializeField]
    Collider area;

    private bool clicked = false;

    public void SetClickable(bool active)
    {
        area.enabled = active;
    }

    void OnMouseDown()
    {
        clicked = true;
        OnPick.Invoke(card);
    }
}
