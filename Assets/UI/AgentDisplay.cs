using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AgentDisplay : MonoBehaviour
{
    public Image portrait;
    public TextMeshProUGUI agentName;
    public TextMeshProUGUI playerProgress;
    public TextMeshProUGUI clueCardCount;
    public TextMeshProUGUI actionCardCount;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setupDisplay(Agent agent, int totalSize)
    {
        agentName.text = agent.agentName;
        // TODO something with groupings
        playerProgress.text = Utility.AbbreviatedNumber(totalSize).Item1;

        setClueCardCount(0);
        setActionCardCount(0);
    }

    public void setClueCardCount(int newCount)
    {
        clueCardCount.text = "x" + newCount;
    }

    public void setActionCardCount(int newCount)
    {
        actionCardCount.text = "x" + newCount;
    }

    public void setProgression(int newCount)
    {
        // TODO something with groupings
        Debug.Log("The new count is " + newCount);
        playerProgress.text = Utility.AbbreviatedNumber(newCount).Item1;
    }
}
