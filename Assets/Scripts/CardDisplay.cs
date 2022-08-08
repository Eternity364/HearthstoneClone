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
    private Canvas textCanvas;
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

    private CardData data;

    public enum DisplayStates
    {
        InHand = 1,
        OnField = 2
    }


    void Update()
    {
        TextVisibilityWorkaround();
    }

    public void SetData(CardData data)
    {
        //if (this.data == null)
            this.data = data;
        UpdateDisplay();
    }

    public void ChangeState(DisplayStates state) {
        bool isHand = state == DisplayStates.InHand;
        inHandAttack.gameObject.SetActive(isHand);
        inHandHealth.gameObject.SetActive(isHand);
        inHandCost.gameObject.SetActive(isHand);
        onFieldAttack.gameObject.SetActive(!isHand);
        onFieldHealth.gameObject.SetActive(!isHand);
        cardInHand.SetActive(isHand);
        cardOnField.SetActive(!isHand);
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

    // Workaround to fix card text visibility after rotating card
    void TextVisibilityWorkaround()
    {
        if (card != null) {
            Debug.Log(card.transform.rotation.y);
            textCanvasGO.SetActive(card.transform.rotation.y > -0.7 && card.transform.rotation.y < 0.7);
            if (card.transform.rotation.y != 0)
                textCanvas.sortingLayerName = "1";
            else
                textCanvas.sortingLayerName = "Default";
        }
    }
}
