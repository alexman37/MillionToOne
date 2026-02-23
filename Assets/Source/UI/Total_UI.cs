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

    private Coroutine activeCo = null;
    

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

    // Doing this with a small delay ensures we don't "click through" elements into other ones that shouldn't be active for that click
    public void changeUIState(Current_UI_State newState)
    {
        uiState = Current_UI_State.Unknown;
        if (activeCo == null)
        {
            activeCo = StartCoroutine(changeUIStateAfterDelay(newState, 0.25f));
        } else
        {
            StopCoroutine(activeCo);
            activeCo = StartCoroutine(changeUIStateAfterDelay(newState, 0.25f));
        }
    }

    private IEnumerator changeUIStateAfterDelay(Current_UI_State newState, float time)
    {
        yield return new WaitForSeconds(time);
        uiState = newState;
        uiStateChanged.Invoke(uiState);
        activeCo = null;
    }
}
