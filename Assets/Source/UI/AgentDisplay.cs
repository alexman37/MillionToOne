using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AgentDisplay : ConditionalUI
{
    public Image portrait;
    public TextMeshProUGUI agentName;
    public TextMeshProUGUI playerProgress;
    public TextMeshProUGUI clueCardCount;
    public TextMeshProUGUI actionCardCount;

    private Image bottom;
    [SerializeField] private Sprite emptySpr;
    [SerializeField] private Sprite hoveredSpr;
    [SerializeField] private Sprite selectedSpr;

    private Agent agent;
    private bool selected;

    public static event Action<int> selectedAgent = (_) => { };
    public static event Action deselectedAgent = () => { };

    // Start is called before the first frame update
    void Start()
    {
        bottom = this.GetComponent<Image>();

        allowedGameStates = new HashSet<Current_UI_State>() { Current_UI_State.PlayerTurn, Current_UI_State.AgentSelection };
    }

    private void OnEnable()
    {
        Total_UI.uiStateChanged += onUIstateUpdate;
        selectedAgent += DeselectAllOthers;
    }

    private void OnDisable()
    {
        Total_UI.uiStateChanged -= onUIstateUpdate;
        selectedAgent -= DeselectAllOthers;
    }

    public void setupDisplay(Agent agent, int totalSize)
    {
        this.agent = agent;
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


    public void OnHover()
    {
        if (activeUI && !selected)
        {
            bottom.sprite = hoveredSpr;
        }
    }

    public void OnHoverEnd()
    {
        if(activeUI && !selected)
        {
            bottom.sprite = emptySpr;
        }
    }

    public void OnSelect()
    {
        if(activeUI)
        {
            selected = true;
            bottom.sprite = selectedSpr;

            selectedAgent.Invoke(agent.id);
        }
    }

    public void OnDeselect()
    {
        if(activeUI)
        {
            selected = false;
            bottom.sprite = emptySpr;

            deselectedAgent.Invoke();
        }
    }

    private void DeselectAllOthers(int id)
    {
        if(activeUI && agent.id != id)
        {
            selected = false;
            bottom.sprite = emptySpr;
        }
    }

    public void OnClick()
    {
        if(activeUI)
        {
            if (selected) OnDeselect();
            else OnSelect();
        }
    }
}
