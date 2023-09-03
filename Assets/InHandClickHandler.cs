using UnityEngine;
using UnityEngine.Events;

public class InHandClickHandler : MonoBehaviour
{
    public UnityAction<Card> OnPick;
    public UnityAction<Card> OnMouseEnterCallbacks;
    public UnityAction<Card> OnMouseLeaveCallbacks;

    [SerializeField]
    public Card card;
    [SerializeField]
    public CardPickedRotationManager rotationManager;
    [SerializeField]
    BoxCollider2D area;

    private bool clicked = false;

    public void SetClickable(bool active)
    {
        area.enabled = active;
    }

    void OnMouseDown()
    {
        clicked = true;
        rotationManager.SetActive(true);
        if (OnPick != null)
            OnPick.Invoke(card);
    }

    void OnMouseEnter()
    {
        if (OnMouseEnterCallbacks != null)
            OnMouseEnterCallbacks.Invoke(card);
    }

    void OnMouseExit()
    {
        if (OnMouseLeaveCallbacks != null)
            OnMouseLeaveCallbacks.Invoke(card);
    }
}
