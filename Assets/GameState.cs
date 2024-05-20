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

    public GameState(List<Card> playerCards, List<Card> opponentCards)
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
    }

    public void Attack(PlayerState attackerState, int attackerIndex, PlayerState targetState, int targetIndex) {
        Assert.IsTrue(attackerState != targetState);
        List<CardData> attackerCardsData = GetListByState(attackerState);
        List<CardData> targetCardsData = GetListByState(targetState);;
        if (attackerState == PlayerState.Enemy)
        {
            attackerCardsData = opponentCardsData;
            targetCardsData = playerCardsData;
        }

        CardData attackerData = attackerCardsData[attackerIndex];
        CardData targetData = targetCardsData[targetIndex];
        targetData.Health -= attackerData.Attack;
        attackerData.Health -= targetData.Attack;
        if (targetData.Health <= 0) {
            targetCardsData.RemoveAt(targetIndex);
        }
        if (attackerData.Health <= 0) {
            attackerCardsData.RemoveAt(attackerIndex);
        }
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
