using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterCard : ConditionalUI
{
    public int characterId;

    public static event Action<int> charCardClicked = (_) => { };

    // Start is called before the first frame update
    void Start()
    {
        // TODO: target selection only
        allowedGameStates = new HashSet<Current_UI_State>() { Current_UI_State.PlayerTurn, Current_UI_State.TargetSelection };
    }

    private void OnEnable()
    {
        Total_UI.uiStateChanged += onUIstateUpdate;
    }

    private void OnDisable()
    {
        Total_UI.uiStateChanged -= onUIstateUpdate;
    }

    public void OnClick()
    {
        if(activeUI)
        {
            charCardClicked.Invoke(characterId);
        }
    }
}
