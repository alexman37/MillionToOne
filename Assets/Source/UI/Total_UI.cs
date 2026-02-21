using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// For managing UI things at the highest level.
/// </summary>
public class Total_UI : MonoBehaviour
{
    public static Current_UI_State uiState = Current_UI_State.Unknown;
    public static event Action<Current_UI_State> uiStateChanged;

    public static Total_UI instance;

    public Inventory inventory;
    public Image deckStation;
    public Image suspectView;
    public Image clueForm;
    public Image numSuspects;
    public Playerbase playerbase;
    

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void initializeUI(List<Agent> agentsInOrder, Roster rost)
    {
        playerbase.initialize(agentsInOrder, rost.simulatedTotalRosterSize);
    }

    public void changeUIState(Current_UI_State newState)
    {
        uiState = newState;
        uiStateChanged.Invoke(uiState);
    }
}
