using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject card;
    [SerializeField]
    private GameObject cardInHand;
    [SerializeField]
    private GameObject cardOnField;
    [SerializeField]
    private GameObject textCanvasGO;
    [SerializeField]
    private GameObject inHandTextGO;
    [SerializeField]
    private GameObject onFieldTextGO;
    [SerializeField]
    private Canvas textCanvas;
    [SerializeField]
    private Canvas inHandCanvas;
    [SerializeField]
    private Canvas onFieldCanvas;
    [SerializeField]
    private Text inHandAttack;
    [SerializeField]
    private Text inHandHealth;
    [SerializeField]
    private Text inHandCost;
    [SerializeField]
    private Text onFieldAttack;
    [SerializeField]
    private Text onFieldHealth;
    [SerializeField]
    private GameObject cardBack;
    [SerializeField]
    private CardData data;
    [SerializeField]
    private CardRenderOrderSetter cardRenderOrderSetter;

    public enum DisplayStates
    {
        InHand = 1,
        OnField = 2
    }


    void Update()
    {
        OnCardTurningVisibility();
    }

    public void SetData(CardData data)
    {
        //if (this.data == null)
            this.data = data;
        UpdateDisplay();
    }

    public void ChangeState(DisplayStates state) {
        bool isHand = state == DisplayStates.InHand;
        inHandTextGO.SetActive(isHand);
        onFieldTextGO.SetActive(!isHand);
        cardInHand.SetActive(isHand);
        cardOnField.SetActive(!isHand);
    }

    public void SetRenderOrder(int order)
    {
        cardRenderOrderSetter.Set(order);
    }

    private void UpdateDisplay() {
        if (this.data != null) {
            inHandAttack.text = data.Attack.ToString();
            inHandHealth.text = data.Health.ToString();
            inHandCost.text = data.Cost.ToString();
            onFieldAttack.text = data.Attack.ToString();
            onFieldHealth.text = data.Health.ToString();
        }
    }

    void OnCardTurningVisibility()
    {
        if (card != null) {
            cardBack.SetActive(!(card.transform.rotation.y > -0.61 && card.transform.rotation.y < 0.79));
            textCanvasGO.SetActive((card.transform.rotation.y > -0.61 && card.transform.rotation.y < 0.79));
        }
    }
}
