using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerbase : MonoBehaviour
{
    public GameObject agentDisplayTemplate;
    private List<AgentDisplay> agentDisplays = new List<AgentDisplay>();
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
        for(int i = 0; i < agentsInOrder.Count; i++)
        {
            GameObject go = GameObject.Instantiate(agentDisplayTemplate, transform);
            go.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, 85 * i);

            AgentDisplay ad = go.GetComponent<AgentDisplay>();
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
