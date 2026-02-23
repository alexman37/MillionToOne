using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    [SerializeField] GameObject clueCardTemplate;
    [SerializeField] GameObject actionCardTemplate;
    [SerializeField] GameObject goldCardTemplate;

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

    public void addCardToInventory(Card c, int cardCount)
    {
        GameObject newCard;

        if (c.cardType == CardType.CLUE)
        {
            newCard = GameObject.Instantiate(clueCardTemplate);
            ClueCard data = c as ClueCard;
            clueCardInstances.Add(newCard);

            PhysicalClueCard pc = newCard.GetComponent<PhysicalClueCard>();
            pc.setData(data);
            data.setPhysical(pc);

            pc.transform.parent = this.transform;
            pc.transform.localPosition = new Vector3(cardCount * 50, 0, -1 * cardCount);
            pc.initialize();
        }

        else if(c.cardType == CardType.ACTION)
        {
            newCard = GameObject.Instantiate(actionCardTemplate);
            ActionCard data = c as ActionCard;
            actionCardInstances.Add(newCard);

            PhysicalActionCard pc = newCard.GetComponent<PhysicalActionCard>();
            pc.setData(data);
            data.setPhysical(pc);

            pc.transform.parent = this.transform;
            pc.transform.localPosition = new Vector3(700 + cardCount * 50, 0, -20 + -1 * cardCount);
            pc.initialize();
        }

        else
        {
            newCard = GameObject.Instantiate(goldCardTemplate);
            GoldCard data = c as GoldCard;
            actionCardInstances.Add(newCard);

            PhysicalActionCard pc = newCard.GetComponent<PhysicalActionCard>();
            pc.setData(data);
            data.setPhysical(pc);

            pc.transform.parent = this.transform;
            pc.transform.localPosition = new Vector3(700 + cardCount * 50, 0, -20 + -1 * cardCount);
            pc.initialize();
        }
        
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
            GameObject toDestroy = actionCardInstances[idx];
            actionCardInstances.RemoveAt(idx);
            Destroy(toDestroy);

            for (int i = idx; i < actionCardInstances.Count; i++)
            {
                actionCardInstances[i].transform.parent = this.transform;
                actionCardInstances[i].GetComponent<PhysicalCard>().bumpBackOne();
            }
        }
    }

    public List<GameObject> getCardsForSelectionWindow(int whatKinds)
    {
        List<GameObject> cardCopies = new List<GameObject>();
        if(whatKinds != 1)
        {
            cardCopies.AddRange(clueCardInstances);
        }
        if(whatKinds != 0)
        {
            cardCopies.AddRange(actionCardInstances);
        }

        return cardCopies;
    }
}
