using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class GameState
{
    public List<CardData> playerCardsData;
    public List<CardData> opponentCardsData;
    [NonSerialized]
    public Action<PlayerState, int> OnCardDead;

    public GameState(List<Card> playerCards, List<Card> opponentCards, Action<PlayerState, int> OnCardDead)
    {
        playerCardsData = new List<CardData>();
        opponentCardsData = new List<CardData>();

        for (int i = 0; i < playerCards.Count; i++)
        {
            CardData data = playerCards[i].GetData();
            playerCardsData.Add(new CardData(data.MaxHealth, data.Attack, data.Cost));
            playerCardsData[i].Health = data.Health;
        }

        for (int i = 0; i < opponentCards.Count; i++)
        {
            CardData data = opponentCards[i].GetData();
            opponentCardsData.Add(new CardData(data.MaxHealth, data.Attack, data.Cost));
            opponentCardsData[i].Health = data.Health;
        }

        this.OnCardDead = OnCardDead;
    }

    private GameState(List<CardData> playerCards, List<CardData> opponentCards)
    {
        playerCardsData = new List<CardData>();
        opponentCardsData = new List<CardData>();

        for (int i = 0; i < playerCards.Count; i++)
        {
            CardData data = playerCards[i];
            playerCardsData.Add(new CardData(data.MaxHealth, data.Attack, data.Cost));
            playerCardsData[i].Health = data.Health;
        }

        for (int i = 0; i < opponentCards.Count; i++)
        {
            CardData data = opponentCards[i];
            opponentCardsData.Add(new CardData(data.MaxHealth, data.Attack, data.Cost));
            opponentCardsData[i].Health = data.Health;
        }

        this.OnCardDead = OnCardDeadEmpty;
    }

    public void Attack(PlayerState attackerState, int attackerIndex, PlayerState targetState, int targetIndex) {
        Assert.IsTrue(attackerState != targetState);
        List<CardData> attackerCardsData = GetListByState(attackerState);
        List<CardData> targetCardsData = GetListByState(targetState);;

        CardData attackerData = attackerCardsData[attackerIndex];
        CardData targetData = targetCardsData[targetIndex];
        targetData.Health -= attackerData.Attack;
        attackerData.Health -= targetData.Attack;
        if (targetData.Health <= 0) {
            targetCardsData.RemoveAt(targetIndex);
            OnCardDead(targetState, targetIndex);
        }
        if (attackerData.Health <= 0) {
            attackerCardsData.RemoveAt(attackerIndex);
            OnCardDead(attackerState, attackerIndex);
        }
    }

    public void OnCardDeadEmpty(PlayerState state, int index) {
    }

    public GameState GetReveresed() {
        return new GameState(opponentCardsData, playerCardsData);
    }

    public List<CardData> GetListByState(PlayerState state) {
        if (state == PlayerState.Player)
            return playerCardsData;
        else
            return opponentCardsData; 
    }

    public string ToJson() {
        return JsonUtility.ToJson(this); 
    }

    public byte[] GetHash() {
        return SecurityHelper.Hash(ToJson());
    }

    public string GetStringHash() {
        return SecurityHelper.GetHexStringFromHash(GetHash());
    }
}
