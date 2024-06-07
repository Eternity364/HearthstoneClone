using System;
using UnityEngine;

[Serializable]
public class HeroData
{
    [SerializeField] private int health;
    [SerializeField] public int maxHealth;
    
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
        get { 
            return maxHealth; 
        }
    }
}
