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
    public GameObject intermediateParent; 
    [SerializeField]
    public InHandClickHandler clickHandler;
    [SerializeField]
    public CardPickedRotationManager rotationManager;

    public GameObject IntermediateParent
    {
        get => intermediateParent;
    }
    public CardPickedRotationManager RotationManager
    {
        get => rotationManager;
    }

    // Start is called before the first frame update
    void Start()
    {
        data = new CardData(2, 3, 4);
        cardDisplay.SetData(data);
    }
}
