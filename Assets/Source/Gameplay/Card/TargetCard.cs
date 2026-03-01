using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class TargetCard : ConditionalUI
{
    [SerializeField] private TextMeshProUGUI cpdNum;
    [SerializeField] private TextMeshProUGUI cpdTitle;
    [SerializeField] private TextMeshProUGUI category;
    [SerializeField] private Sprite pic;
    [SerializeField] private SpriteRenderer seal;

    private CPD_Type cpdType;

    [SerializeField] private Image selectedSpr;

    private bool revealed;
    TargetCPDGuessReward reward;

    // Start is called before the first frame update
    void Start()
    {
        allowedGameStates = new HashSet<Current_UI_State>() { Current_UI_State.PlayerTurn };
    }

    private void OnEnable()
    {
        Total_UI.uiStateChanged += onUIstateUpdate;
        TargetCharGuess.playerGuessesTargetProperty += OnPlayerGuess;
    }

    private void OnDisable()
    {
        Total_UI.uiStateChanged -= onUIstateUpdate;
        TargetCharGuess.playerGuessesTargetProperty -= OnPlayerGuess;
    }

    public void initialize(int num, string title, string cat, TargetCPDGuessReward award)
    {
        cpdNum.text = (num + 1).ToString();
        cpdTitle.text = title;
        category.text = cat;
        reward = award;

        switch(reward)
        {
            case TargetCPDGuessReward.ActionCard:
                seal.sprite = TargetProperties.instance.actionCardSeal;
                break;
            case TargetCPDGuessReward.GoldCard:
                seal.sprite = TargetProperties.instance.goldCardSeal;
                break;
        }

        cpdType = Roster.cpdConstrainables[num].cpdType;
    }

    private void OnPlayerGuess(CPD_Type cpdType, string _, bool wasCorrect)
    {
        if (wasCorrect && cpdType == this.cpdType)
        {
            RevealToAll();
            TurnDriver.instance.giveReward(0, reward);
        }
    }


    // When it's correctly guessed, reveal it to all players (and prevent future action)
    private void RevealToAll()
    {
        transform.parent.localRotation = Quaternion.Euler(0, 0, 0);
        revealed = true;
    }


    public void OnMouseEnter()
    {
        if (activeUI && !revealed)
        {
            selectedSpr.gameObject.SetActive(true);
        }
    }

    public void OnMouseExit()
    {
        if(activeUI)
        {
            selectedSpr.gameObject.SetActive(false);
        }
    }

    public void OnMouseDown()
    {
        if(activeUI && !revealed)
        {
            Total_UI.instance.changeUIState(Current_UI_State.GuessingCPD);
            PopupCanvas.instance.popup_targetPropertyGuess(cpdType);
        }
    }
}
