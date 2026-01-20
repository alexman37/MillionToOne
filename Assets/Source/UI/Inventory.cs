using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    [SerializeField] GameObject clueCardTemplate;
    [SerializeField] GameObject actionCardTemplate;

    // Start is called before the first frame update
    private void OnEnable()
    {
        PlayerAgent.playerGotCard += addCardToInventory;
    }

    private void OnDisable()
    {
        PlayerAgent.playerGotCard -= addCardToInventory;
    }

    public void addCardToInventory(Card c, int _)
    {
        GameObject newCard;
        Card data;

        if (c.cardType == CardType.CLUE)
        {
            newCard = GameObject.Instantiate(clueCardTemplate);
            data = c as ClueCard;
        }

        else
        {
            newCard = GameObject.Instantiate(actionCardTemplate);
            data = c as ActionCard;
        }

        PhysicalCard pc = newCard.GetComponent<PhysicalCard>();
        pc.setData(data);

        pc.initialize();

        int cardCount = PlayerAgent.instance.getCardsCount();

        pc.transform.parent = this.transform;
        pc.transform.localPosition = new Vector3(cardCount * 50, 0, -1 * cardCount);
        // TODO set position
    }
}
