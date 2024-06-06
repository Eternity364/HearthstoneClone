using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardData
{
    [SerializeField] int index;
    [SerializeField] private int health, attack, cost;
    [SerializeField] public int maxHealth;
    [SerializeField] private bool active = true;
    [SerializeField] public List<Buff> buffs = new List<Buff>();
    [SerializeField] public List<Ability> abilities = new List<Ability>();
    [SerializeField] public BattlecryBuff battlecryBuff;
    
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
            int value = maxHealth;
            foreach (var item in buffs)
            {
                value += item.health;
            }
            return value; 
        }
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

    public CardData(int health, int maxHealth, int attack, int cost, int index, List<Ability> abilities, List<Buff> buffs, BattlecryBuff battlecryBuff)
    {
        Health = health;
        Attack = attack;
        Cost = cost;
        this.maxHealth = maxHealth;
        this.index = index;

        this.abilities = new List<Ability>();
        for (int i = 0; i < abilities.Count; i++)
        {
            this.abilities.Add(abilities[i]);
        }
        this.buffs = new List<Buff>();
        for (int i = 0; i < buffs.Count; i++)
        {
            Buff buff = new Buff(buffs[i].health, buffs[i].attack);
            this.buffs.Add(buff);
        }
        if (battlecryBuff != null) {
            Buff buff = new Buff(battlecryBuff.buff.health, battlecryBuff.buff.attack);
            this.battlecryBuff = new BattlecryBuff(buff);
        }

    }

    public bool HasAbility(Ability ability) {
        return abilities.Contains(ability);
    }

    public bool IsAttackBuffed() {
        for (int i = 0; i < buffs.Count; i++)
        {
            if (buffs[i].attack > 0)
                return true;
        }
        return false;
    }

    public bool IsHealthBuffed() {
        for (int i = 0; i < buffs.Count; i++)
        {
            if (buffs[i].health > 0)
                return true;
        }
        return false;
    }


    public void AddBuff(Buff buff)
    {
        buffs.Add(buff);
        health += buff.health;
        attack += buff.attack;
        Debug.Log("Health = " + health);
        Debug.Log("Attack = " + attack);
    }
}

[Serializable]
public class Buff {
    public int health, attack;

    public Buff(int health, int attack)
    {
        this.health = health;
        this.attack = attack;
    }
}

[Serializable]
public enum Ability
{
    BattlecryBuff
}

[Serializable]
public class BattlecryBuff{
    public Buff buff;

    public BattlecryBuff(Buff buff)
    {
        this.buff = buff;
    }
}
