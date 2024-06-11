using UnityEngine;

public class Card : MonoBehaviour
{
    public AngleSetter angleSetter;

    [SerializeField]
    public CardDisplay display;
    [SerializeField]
    public InHandClickHandler clickHandler;
    [SerializeField]
    private CardPickedRotationManager rotationManager;

    public CardPickedRotationManager RotationManager
    {
        get => rotationManager;
    }

    public bool TryAndApplyCharge (InputBlock block) {
        if (display.Data.abilities.Contains(Ability.Charge)) {
            display.Data.abilities.Remove(Ability.Charge);
            InputBlockerInstace.Instance.RemoveBlock(block);
            return true;
        }
        return false;
    }
    
    public bool DealDamage(int damage)
    {
        if (display.Data.abilities.Contains(Ability.DivineShield)) {
            display.RemoveDivineShield();
        }
        else
        {
            display.Data.Health -= damage;
            display.ShowDamage(damage);
        }

        display.UpdateDisplay();
        bool dead = display.Data.Health <= 0;
        if (dead) StartDeathAnimation();
        return dead;
    }

    public CardData GetData() {
        return display.Data;
    }
    
    void StartDeathAnimation()
    {
        display.StartDeathAnimation();
        //Destroy(this.gameObject);
    }
}
