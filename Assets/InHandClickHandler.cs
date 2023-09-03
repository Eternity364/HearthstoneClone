using UnityEngine;
using UnityEngine.Events;

public class InHandClickHandler : MonoBehaviour
{
    public UnityAction<Card> OnPick;
    public UnityAction<Card> OnMouseUpEvents;
    public UnityAction<Card> OnMouseEnterCallbacks;
    public UnityAction<Card> OnMouseLeaveCallbacks;

    [SerializeField]
    public Card card;
    [SerializeField]
    public CardPickedRotationManager rotationManager;
    [SerializeField]
    BoxCollider2D areaRect;
    [SerializeField]
    CapsuleCollider2D areaCapsule;

    bool mouseEntered = false;

    public void SetClickable(bool active)
    {
        areaRect.enabled = active;
        areaCapsule.enabled = active;
    }

    public void SetRectAreaClickable(bool active)
    {
        areaRect.enabled = active;
    }

    void OnMouseDown()
    {
        rotationManager.SetActive(true);
        if (OnPick != null)
            OnPick.Invoke(card);
    }

    // void OnMouseUp()
    // {
    //     print("yep");
    //     if (OnMouseUpEvents != null)
    //         OnMouseUpEvents.Invoke(card);
    // }

    void OnMouseEnter()
    {
        mouseEntered = true;
        if (OnMouseEnterCallbacks != null)
            OnMouseEnterCallbacks.Invoke(card);
    }

    void OnMouseExit()
    {
        mouseEntered = false;
        if (OnMouseLeaveCallbacks != null)
            OnMouseLeaveCallbacks.Invoke(card);
    }

    void Update() {
        if (Input.GetMouseButtonUp (0) && mouseEntered) {
            if (OnMouseUpEvents != null)
                OnMouseUpEvents.Invoke(card);
        }
    }
}
