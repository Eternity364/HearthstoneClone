using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGenerator : MonoBehaviour
{
    [SerializeField]
    List<Card> cardsSet;

    public Card Create(int index, Transform parent)
    {
        Card card = Instantiate(cardsSet[index - 1], parent);
        card.gameObject.SetActive(true);
        return card;
    }

    
    public List<CardData> GetRandomDataList(int count)
    {
        List<CardData> list = new List<CardData>();
        for (int i = 0; i < count; i++)
        {
            list.Add(GetRandomData());
        }
        return list;
    }

    public CardData GetRandomData()
    {
        CardData data = cardsSet[Random.Range(0, cardsSet.Count)].GetData();
        return new CardData(data.Health, data.maxHealth, data.Attack, data.Cost, data.Index, data.abilities, data.buffs, data.battlecryBuff);
    }
}
