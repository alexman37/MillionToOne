using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerbase : MonoBehaviour
{
    public GameObject agentDisplayTemplate;
    [SerializeField] private List<AgentDisplay> agentDisplays = new List<AgentDisplay>();

    public GameObject turnMarker;


    private void OnEnable()
    {
        PlayerAgent.playerGotCard += playerGetsCard;
        PlayerAgent.playerUpdateProgress += playerProgressChange;
        CPUAgent.cpuGotCard += cpuGetsCard;
        CPUAgent.cpuUpdateProgress += cpuProgressChange;
    }

    private void OnDisable()
    {
        PlayerAgent.playerGotCard -= playerGetsCard;
        PlayerAgent.playerUpdateProgress -= playerProgressChange;
        CPUAgent.cpuGotCard -= cpuGetsCard;
        CPUAgent.cpuUpdateProgress -= cpuProgressChange;
    }

    public IEnumerator initialize(List<Agent> agentsInOrder, int totalSize)
    {
        for(int i = 0; i < agentsInOrder.Count; i++)
        {
            AgentDisplay ad = agentDisplays[i].GetComponent<AgentDisplay>();
            ad.gameObject.SetActive(true);
            ad.setupDisplay(agentsInOrder[i], totalSize);
            agentDisplays.Add(ad);
            yield return new WaitForSeconds(0.3f);
        }
    }

    // HANDLERS
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

    
    // Turn Marker
    public void moveTurnMarker(int toId)
    {
        StartCoroutine(moveTurnMarkerCo(toId));
    }

    IEnumerator moveTurnMarkerCo(int toId)
    {
        RectTransform rect = turnMarker.GetComponent<RectTransform>();

        Vector3 start = rect.anchoredPosition;
        Vector3 temp = new Vector3(727, 250, 0);
        Vector3 end = temp - new Vector3(0, toId * 70, 0);
        float maxTime = 1;

        for (float t = 0; t < maxTime; t += Time.deltaTime)
        {
            yield return rect.anchoredPosition = UITransitions.Xerp(start, end, t / maxTime);
            Debug.Log("Pos " + rect.anchoredPosition);
        }
        rect.anchoredPosition = end;
    }
}
