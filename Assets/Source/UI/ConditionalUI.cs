using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI that can only be interacted with in certain game states.
/// Specify the states in "allowedGameStates"
/// </summary>
public class ConditionalUI : MonoBehaviour
{
    public HashSet<Current_UI_State> allowedGameStates;
    public bool activeUI;

    private void OnEnable()
    {
        Total_UI.uiStateChanged += onUIstateUpdate;
    }

    private void OnDisable()
    {
        Total_UI.uiStateChanged -= onUIstateUpdate;
    }

    public virtual void onUIstateUpdate(Current_UI_State newState)
    {
        // If we forgot to specify allowed states, or haven't set them yet, assume they are all OK
        if (allowedGameStates == null) activeUI = true;
        else activeUI = allowedGameStates.Contains(newState);
    }
}

public enum Current_UI_State
{
    Unknown,
    PlayerTurn,
    CPUTurn,
    GenTransition, // Generic Transition / Animation that usually allows for nothing
    SelectionWindow,
    ReactionWindow,
    AgentSelection,
    GuessingCPD,
    TargetSelection
}