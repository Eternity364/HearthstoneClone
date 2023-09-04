using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField]
    int health;
    [SerializeField]
    int attack;
    [SerializeField]
    int cost;
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

    void Start()
    {
        cardDisplay.SetData(new CardData(health, attack, cost));
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
