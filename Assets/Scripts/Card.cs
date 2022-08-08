using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public CardData data;

    [SerializeField]
    public CardDisplay cardDisplay;

    // Start is called before the first frame update
    void Start()
    {
        data = new CardData(2, 3, 4);
        cardDisplay.SetData(data);
        cardDisplay.ChangeState(CardDisplay.DisplayStates.OnField);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
