using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

[Serializable]
public class GameState
{
    public List<CardData> playerCardsData;
    public List<CardData> opponentCardsData;
    public List<CardData> playerCardsInHandData;
    public List<CardData> opponentCardsInHandData;
    public int currentPlayerMana, playerMana, playerMaxMana;
    public int currentOpponentMana, opponentMana, opponentMaxMana;
    [NonSerialized]
    public UnityAction<PlayerState, int> OnCardDead;
    [NonSerialized]
    public UnityAction<PlayerState, int, int> OnManaChange;

    public GameState(List<Card> playerCards, List<Card> opponentCards, 
        List<Card> playerCardsInHand, List<Card> opponentCardsInHand, 
        int playerMana, int opponentMana, int playerMaxMana, int opponentMaxMana,
        UnityAction<PlayerState, int> OnCardDead, UnityAction<PlayerState, int, int> OnManaChange)
    {
        playerCardsData = new List<CardData>();
        opponentCardsData = new List<CardData>();
        playerCardsInHandData = new List<CardData>();
        opponentCardsInHandData = new List<CardData>();

        AddDataFromCardList(playerCards, playerCardsData);
        AddDataFromCardList(opponentCards, opponentCardsData);
        AddDataFromCardList(playerCardsInHand, playerCardsInHandData);
        AddDataFromCardList(opponentCardsInHand, opponentCardsInHandData);

        this.playerMana = playerMana;
        this.opponentMana = opponentMana;
        this.playerMaxMana = playerMaxMana;
        this.opponentMaxMana = opponentMaxMana;
        this.currentPlayerMana = playerMana;
        this.currentOpponentMana = opponentMana;
        this.OnCardDead += OnCardDead;
        this.OnManaChange += OnManaChange;
        Update();
    }
    

    public GameState(List<CardData> playerCards, List<CardData> opponentCards, List<CardData> playerCardsInHand, List<CardData> opponentCardsInHand,
        int playerMana, int opponentMana, int currentPlayerMana, int currentOpponentMana, int playerMaxMana, int opponentMaxMana,
        UnityAction<PlayerState, int> OnCardDead, UnityAction<PlayerState, int, int> OnManaChange)
    {
        playerCardsData = new List<CardData>();
        opponentCardsData = new List<CardData>();
        playerCardsInHandData = new List<CardData>();
        opponentCardsInHandData = new List<CardData>();

        AddDataFromCardDataList(playerCards, playerCardsData);
        AddDataFromCardDataList(opponentCards, opponentCardsData);
        AddDataFromCardDataList(playerCardsInHand, playerCardsInHandData);
        AddDataFromCardDataList(opponentCardsInHand, opponentCardsInHandData);
        
        this.playerMana = playerMana;
        this.opponentMana = opponentMana;
        this.playerMaxMana = playerMaxMana;
        this.opponentMaxMana = opponentMaxMana;
        this.currentPlayerMana = currentPlayerMana;
        this.currentOpponentMana = currentOpponentMana;

        this.OnCardDead += OnCardDead;
        this.OnManaChange += OnManaChange;
        Update();
    }

    public void ProgressMana(PlayerState state) {
        if (state == PlayerState.Player) {
            playerMana++;
            if (playerMana > playerMaxMana)
                playerMana = playerMaxMana;
            currentPlayerMana = playerMana;
            OnManaChange.Invoke(state, currentPlayerMana,  playerMana);
        } else {
            opponentMana++;
            if (opponentMana > opponentMaxMana)
                opponentMana = opponentMaxMana;
            currentOpponentMana = opponentMana;
            OnManaChange.Invoke(state, currentOpponentMana,  opponentMana);
        }
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

    
    public void ApplyBuff(PlayerState state, int casterIndex, int targetIndex) {
        List<CardData> list = GetListByState(state);

        CardData casterData = list[casterIndex];
        CardData targetData = list[targetIndex];
        Buff buff = casterData.battlecryBuff.buff;
        Buff buffCopy = new Buff(buff.health, buff.attack);
        targetData.AddBuff(buffCopy);
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
        SpendMana(side, data.Cost);
        data.Active = false;
    }

    public bool IsEnoughMana(PlayerState state, int manaSpent) {
        if (state == PlayerState.Player) {
            return currentPlayerMana - manaSpent >= 0;
        } else {
            return currentOpponentMana - manaSpent >= 0;
        }
    }

    public void SpendMana(PlayerState state, int manaSpent) {
        if (state == PlayerState.Player) {
            currentPlayerMana -= manaSpent;
            OnManaChange.Invoke(state, currentPlayerMana,  playerMana);
        } else {
            currentOpponentMana -= manaSpent;
            OnManaChange.Invoke(state, currentOpponentMana,  opponentMana);
        }
    }
    
    private void OnCardDeadEmpty(PlayerState state, int index) {
    }

    private void OnManaChangeEmpty(PlayerState state, int empty, int empty2) {
    }

    public void Update() {
        OnManaChange(PlayerState.Player, currentPlayerMana, playerMana);
        OnManaChange(PlayerState.Enemy, currentOpponentMana, opponentMana);
    }

    public GameState GetReveresed() {
        return new GameState(opponentCardsData, playerCardsData, opponentCardsInHandData, playerCardsInHandData, 
            opponentMana, playerMana, currentOpponentMana, currentPlayerMana, opponentMaxMana, playerMaxMana,
            OnCardDeadEmpty, OnManaChangeEmpty);
    }

    public List<CardData> GetListByState(PlayerState state) {
        if (state == PlayerState.Player)
            return playerCardsData;
        else
            return opponentCardsData; 
    }

    public List<CardData> GetHandListByState(PlayerState state) {
        if (state == PlayerState.Player)
            return playerCardsInHandData;
        else
            return opponentCardsInHandData; 
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
        CardData data = new CardData(from.Health, from.maxHealth, from.Attack, from.Cost, from.Index, from.abilities, from.buffs, from.battlecryBuff);
        data.Health = from.Health;
        data.Active = from.Active;
        return data;
    }
}
