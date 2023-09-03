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
    private CardPickedRotationManager rotationManager;

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
    
    public bool DealDamage(int damage)
    {
        cardDisplay.Data.Health -= damage;
        cardDisplay.UpdateDisplay();
        bool dead = cardDisplay.Data.Health <= 0;
        if (dead) StartDeathAnimation();
        return dead;
    }
    
    void StartDeathAnimation()
    {
        Destroy(this.gameObject);
    }
}
