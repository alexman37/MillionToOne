using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerbase : MonoBehaviour
{
    public GameObject agentDisplayTemplate;
    [SerializeField] private List<AgentDisplay> agentDisplays = new List<AgentDisplay>();
    [SerializeField] private Sprite hoveredAgentSprite;
    [SerializeField] private Sprite selectedAgentSprite;


    private void OnEnable()
    {
        CPUAgent.cpuGotCard += cpuGetsCard;
        CPUAgent.cpuUpdateProgress += cpuProgressChange;
        PlayerAgent.playerGotCard += playerGetsCard;
        PlayerAgent.playerUpdateProgress += playerProgressChange;
    }

    private void OnDisable()
    {
        CPUAgent.cpuGotCard -= cpuGetsCard;
        CPUAgent.cpuUpdateProgress -= cpuProgressChange;
        PlayerAgent.playerGotCard -= playerGetsCard;
        PlayerAgent.playerUpdateProgress -= playerProgressChange;
    }

    public void initialize(List<Agent> agentsInOrder, int totalSize)
    {
        // start at one so you skip the player
        for(int i = 1; i < agentsInOrder.Count; i++)
        {
            AgentDisplay ad = agentDisplays[i-1].GetComponent<AgentDisplay>();
            ad.gameObject.SetActive(true);
            ad.setupDisplay(agentsInOrder[i], totalSize);
            agentDisplays.Add(ad);
        }
    }

    // HANDLERS
    private void cpuGetsCard(int id, Card c, int size)
    {
        if(c.cardType == CardType.CLUE)
            updateClueCardCountForAgent(id, size);
        else updateActionCardCountForAgent(id, size);
    }

    private void cpuProgressChange(int id, int size)
    {
        updateProgression(id, size);
    }

    private void playerGetsCard(Card c, int size)
    {
        if (c.cardType == CardType.CLUE)
            updateClueCardCountForAgent(0, size);
        else updateActionCardCountForAgent(0, size);
    }

    private void playerProgressChange(int size)
    {
        updateProgression(0, size);
    }



    // Set Display
    public void updateClueCardCountForAgent(int id, int numCards)
    {
        agentDisplays[id].setClueCardCount(numCards);
    }

    public void updateActionCardCountForAgent(int id, int numCards)
    {
        agentDisplays[id].setActionCardCount(numCards);
    }

    public void updateProgression(int id, int newRosterSize)
    {
        agentDisplays[id].setProgression(newRosterSize);
    }
}
