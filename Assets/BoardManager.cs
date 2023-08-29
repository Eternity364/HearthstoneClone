using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    List<Card> placingCards = new List<Card>();

    public void PlaceCard(Card card)
    {
        placingCards.Add(card);
        card.gameObject.transform.SetParent(this.gameObject.transform);
        Vector3 position = card.transform.position;
        card.transform.localPosition = new Vector3();
        card.IntermediateParent.transform.position = position;
        print("board");
    }
}
