using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

// Button used for guessing characteristics of the target
public class TargetCharGuess : ConditionalUI
{
    public CPD_Type cpdType;
    public string category;

    [SerializeField] private TextMeshProUGUI text;

    // This should get immediately converted into Agent.targetCharacteristicGuess
    public static event Action<CPD_Type, string, bool> playerGuessesTargetProperty;
    

    // Start is called before the first frame update
    void Start()
    {
        if (playerGuessesTargetProperty == null) playerGuessesTargetProperty += (_, __, b) => { };

        allowedGameStates = new HashSet<Current_UI_State>() { Current_UI_State.GuessingCPD };

        activeUI = true;
    }

    public void initialize(CPD_Type cpdType, string cat)
    {
        this.cpdType = cpdType;
        this.category = cat;
        text.text = cat;
    }

    public void Guess()
    {
        if (!activeUI) return;
        PopupCanvas.instance.popup_targetPropertyClear();

        bool wasCorrect = TurnDriver.instance.currentRoster.targetHasProperty(cpdType, category);
        playerGuessesTargetProperty.Invoke(cpdType, category, wasCorrect);
    }
}
