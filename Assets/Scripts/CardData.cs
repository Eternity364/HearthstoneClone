using System;
using UnityEngine;

[Serializable]
public class CardData
{
    [SerializeField] int index;
    [SerializeField] private int health, attack, cost;
    [SerializeField] private int maxHealth;
    [SerializeField] private bool active = true;
    
    public int Health
    {
        get { return health; }
        set
        {
            health = value;
        }
    }
    public int MaxHealth
    {
        get { return maxHealth; }
    }
    public int Index
    {
        get { return index; }
    }
    public int Attack
    {
        get { return attack; }
        set
        {
            if (value < 0)
                value = 0;
            attack = value;
        }
    }
    public bool Active
    {
        get { return active; }
        set
        {
            active = value;
        }
    }
    public int Cost
    {
        get { return cost; }
        set
        {
            if (value < 0)
                value = 0;
            cost = value;
        }
    }

    public CardData(int health, int attack, int cost, int index)
    {
        Health = health;
        Attack = attack;
        Cost = cost;
        maxHealth = health;
        this.index = index;
    }
}
