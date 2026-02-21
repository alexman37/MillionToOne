using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class TargetCard : ConditionalUI
{
    [SerializeField] private TextMeshProUGUI cpdNum;
    [SerializeField] private TextMeshProUGUI cpdTitle;
    [SerializeField] private TextMeshProUGUI category;
    [SerializeField] private Sprite pic;

    private CPD_Type cpdType;

    [SerializeField] private Image selectedSpr;

    private bool revealed;

    // Start is called before the first frame update
    void Start()
    {
        allowedGameStates = new HashSet<Current_UI_State>() { Current_UI_State.PlayerTurn };
    }

    private void OnEnable()
    {
        Total_UI.uiStateChanged += onUIstateUpdate;
        TargetCharGuess.playerGuessesTargetProperty += RevealToAll;
    }

    private void OnDisable()
    {
        Total_UI.uiStateChanged -= onUIstateUpdate;
        TargetCharGuess.playerGuessesTargetProperty -= RevealToAll;
    }

    public void initialize(int num, string title, string cat)
    {
        cpdNum.text = (num + 1).ToString();
        cpdTitle.text = title;
        category.text = cat;

        cpdType = Roster.cpdConstrainables[num].cpdType;
    }


    // When it's correctly guessed, reveal it to all players (and prevent future action)
    public void RevealToAll(CPD_Type cpdType, string _, bool wasCorrect)
    {
        if (wasCorrect && cpdType == this.cpdType)
        {
            transform.parent.localRotation = Quaternion.Euler(0, 0, 0);
            revealed = true;
        }
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
