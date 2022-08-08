using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public CardData absCard;

    [SerializeField]
    public CardDisplay cardDisplay;

    // Start is called before the first frame update
    void Start()
    {
        absCard = new CardData(1, 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
