using UnityEngine;

public class CardData
{
    private int health, attack, cost;
    private int maxHealth;
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

    public CardData(int health, int attack, int cost)
    {
        Health = health;
        Attack = attack;
        Cost = cost;
        maxHealth = health;
    }
}
