using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public CardData data;
    public AngleSetter angleSetter;

    [SerializeField]
    public CardDisplay cardDisplay;  
    [SerializeField]
    public InHandClickHandler clickHandler;
    [SerializeField]

    // Start is called before the first frame update
    void Start()
    {
        data = new CardData(2, 3, 4);
        cardDisplay.SetData(data);
    }
}
