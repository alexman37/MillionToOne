using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

// Button used for guessing characteristics of the target
public class TargetCharGuess : MonoBehaviour
{
    public CPD_Type cpdType;
    public string category;

    [SerializeField] private TextMeshProUGUI text;

    public static event Action<CPD_Type, string> playerGuessesTargetProperty;
    

    // Start is called before the first frame update
    void Start()
    {
        if (playerGuessesTargetProperty == null) playerGuessesTargetProperty += (_, __) => { };
    }

    public void initialize(CPD_Type cpdType, string cat)
    {
        this.cpdType = cpdType;
        this.category = cat;
        text.text = cat;
    }

    public void Guess()
    {
        PopupCanvas.instance.popup_targetPropertyClear();
        playerGuessesTargetProperty.Invoke(cpdType, category);
    }
}
