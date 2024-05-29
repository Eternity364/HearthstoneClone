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
    public List<CardData> playerCardsInHandData;
    public List<CardData> opponentCardsInHandData;
    [NonSerialized]
    public Action<PlayerState, int> OnCardDead;

    public GameState(List<Card> playerCards, List<Card> opponentCards, 
        List<Card> playerCardsInHand, List<Card> opponentCardsInHand, Action<PlayerState, int> OnCardDead)
    {
        playerCardsData = new List<CardData>();
        opponentCardsData = new List<CardData>();
        playerCardsInHandData = new List<CardData>();
        opponentCardsInHandData = new List<CardData>();

        AddDataFromCardList(playerCards, playerCardsData);
        AddDataFromCardList(opponentCards, opponentCardsData);
        AddDataFromCardList(playerCardsInHand, playerCardsInHandData);
        AddDataFromCardList(opponentCardsInHand, opponentCardsInHandData);

        this.OnCardDead = OnCardDead;
    }
    

    private GameState(List<CardData> playerCards, List<CardData> opponentCards, List<CardData> playerCardsInHand, List<CardData> opponentCardsInHand)
    {
        playerCardsData = new List<CardData>();
        opponentCardsData = new List<CardData>();
        playerCardsInHandData = new List<CardData>();
        opponentCardsInHandData = new List<CardData>();

        AddDataFromCardDataList(playerCards, playerCardsData);
        AddDataFromCardDataList(opponentCards, opponentCardsData);
        AddDataFromCardDataList(playerCardsInHand, playerCardsInHandData);
        AddDataFromCardDataList(opponentCardsInHand, opponentCardsInHandData);

        this.OnCardDead = OnCardDeadEmpty;
    }

    public void SetCardsActive(PlayerState state)
    {
        List<CardData> activeCardsData = GetListByState(state);
        PlayerState inactiveState = PlayerState.Enemy;
        if (state == PlayerState.Enemy)
            inactiveState = PlayerState.Player;

        List<CardData> inactiveCardsData = GetListByState(inactiveState);
        Debug.Log("state = " + state);
        Debug.Log("inactiveState = " + inactiveState);


        for (int i = 0; i < activeCardsData.Count; i++)
        {
            activeCardsData[i].Active = true;
        }        
        for (int i = 0; i < inactiveCardsData.Count; i++)
        {
            inactiveCardsData[i].Active = false;
        }
    }

    public void Attack(PlayerState attackerState, int attackerIndex, PlayerState targetState, int targetIndex) {
        Assert.IsTrue(attackerState != targetState);
        List<CardData> attackerCardsData = GetListByState(attackerState);
        List<CardData> targetCardsData = GetListByState(targetState);;

        CardData attackerData = attackerCardsData[attackerIndex];
        CardData targetData = targetCardsData[targetIndex];
        targetData.Health -= attackerData.Attack;
        attackerData.Health -= targetData.Attack;
        attackerData.Active = false;
        
        if (targetData.Health <= 0) {
            targetCardsData.RemoveAt(targetIndex);
            OnCardDead(targetState, targetIndex);
        }
        if (attackerData.Health <= 0) {
            attackerCardsData.RemoveAt(attackerIndex);
            OnCardDead(attackerState, attackerIndex);
        }
    }

    public void PlaceCard(PlayerState side, int handIndex, int boardIndex) {
        List<CardData> handDatas = playerCardsInHandData;
        List<CardData> boardDatas = playerCardsData;
        if (side == PlayerState.Enemy) {
            handDatas = opponentCardsInHandData;
            boardDatas = opponentCardsData;
        }

        CardData data = handDatas[handIndex];
        handDatas.RemoveAt(handIndex);
        boardDatas.Insert(boardIndex, data);
        data.Active = true;
    }

    public void OnCardDeadEmpty(PlayerState state, int index) {
    }

    public GameState GetReveresed() {
        return new GameState(opponentCardsData, playerCardsData, opponentCardsInHandData, playerCardsInHandData);
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

    // For test purposes
    public void PrintCounts() {
        Debug.Log("playerCardsData = " + playerCardsData.Count);
        Debug.Log("opponentCardsData = " + opponentCardsData.Count);
        Debug.Log("playerCardsInHandData = " + playerCardsInHandData.Count);
        Debug.Log("opponentCardsInHandData = " + opponentCardsInHandData.Count);
    }

    private void AddDataFromCardList(List<Card> from, List<CardData> to) {
        for (int i = 0; i < from.Count; i++)
        {
            
            to.Add(AddData(from[i].GetData()));
        }
    }

    private void AddDataFromCardDataList(List<CardData> from, List<CardData> to) {
        for (int i = 0; i < from.Count; i++)
        {
            to.Add(AddData(from[i]));
        }
    }

    private CardData AddData(CardData from) {
        CardData data = new CardData(from.MaxHealth, from.Attack, from.Cost);
        data.Health = from.Health;
        data.Active = from.Active;
        return data;
    }
}
