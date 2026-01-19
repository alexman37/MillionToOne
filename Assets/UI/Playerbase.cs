using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerbase : MonoBehaviour
{
    public GameObject agentDisplayTemplate;
    private List<AgentDisplay> agentDisplays = new List<AgentDisplay>();

    
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
}
