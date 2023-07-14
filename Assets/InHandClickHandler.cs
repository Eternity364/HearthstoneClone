using UnityEngine;
using UnityEngine.Events;

public class InHandClickHandler : MonoBehaviour
{
    public UnityAction<Card> OnPick;

    [SerializeField]
    public Card card;
    [SerializeField]
    public CardPickedRotationManager rotationManager;
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
        rotationManager.SetActive(true);
        OnPick.Invoke(card);
    }
}
