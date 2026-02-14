using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    [SerializeField] GameObject clueCardTemplate;
    [SerializeField] GameObject actionCardTemplate;

    private List<GameObject> clueCardInstances = new List<GameObject>();
    private List<GameObject> actionCardInstances = new List<GameObject>();

    // Start is called before the first frame update
    private void OnEnable()
    {
        PlayerAgent.playerGotCard += addCardToInventory;
        PlayerAgent.playerLostCard += removeCardFromInventory;
    }

    private void OnDisable()
    {
        PlayerAgent.playerGotCard -= addCardToInventory;
        PlayerAgent.playerLostCard -= removeCardFromInventory;
    }

    public void addCardToInventory(Card c, int _)
    {
        GameObject newCard;
        Card data;

        if (c.cardType == CardType.CLUE)
        {
            newCard = GameObject.Instantiate(clueCardTemplate);
            data = c as ClueCard;
            clueCardInstances.Add(newCard);
        }

        else
        {
            newCard = GameObject.Instantiate(actionCardTemplate);
            data = c as ActionCard;
            actionCardInstances.Add(newCard);
        }

        PhysicalCard pc = newCard.GetComponent<PhysicalCard>();
        pc.setData(data);

        int cardCount = PlayerAgent.instance.getCardsCount();

        pc.transform.parent = this.transform;
        pc.transform.localPosition = new Vector3(cardCount * 50, 0, -1 * cardCount);
        pc.initialize();
    }

    public void removeCardFromInventory(Card c, int idx)
    {
        if(c is ClueCard)
        {
            GameObject toDestroy = clueCardInstances[idx];
            clueCardInstances.RemoveAt(idx);
            Destroy(toDestroy);

            for(int i = idx; i < clueCardInstances.Count; i++)
            {
                clueCardInstances[i].transform.parent = this.transform;
                clueCardInstances[i].GetComponent<PhysicalCard>().bumpBackOne();
            }
        } else
        {
            Debug.LogWarning("No remove for other cards yet.");
        }
    }
}
