using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
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

    public bool TryAndApplyCharge (InputBlock block) {
        if (cardDisplay.Data.abilities.Contains(Ability.Charge)) {
            cardDisplay.Data.abilities.Remove(Ability.Charge);
            InputBlockerInstace.Instance.RemoveBlock(block);
            return true;
        }
        return false;
    }
    
    public bool DealDamage(int damage)
    {
        if (cardDisplay.Data.abilities.Contains(Ability.DivineShield)) {
            cardDisplay.RemoveDivineShield();
        }
        else
        {
            cardDisplay.Data.Health -= damage;
        }

        cardDisplay.UpdateDisplay();
        bool dead = cardDisplay.Data.Health <= 0;
        if (dead) StartDeathAnimation();
        return dead;
    }

    public CardData GetData() {
        return cardDisplay.Data;
    }
    
    void StartDeathAnimation()
    {
        cardDisplay.StartDeathAnimation();
        //Destroy(this.gameObject);
    }
}
