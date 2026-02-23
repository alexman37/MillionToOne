using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class SelectionCard : ConditionalUI
{
    [SerializeField] private TextMeshProUGUI textField1;
    [SerializeField] private TextMeshProUGUI textField2;
    [SerializeField] private TextMeshProUGUI textField3;
    [SerializeField] private Sprite pic;

    [SerializeField] private Image selectedSpr;

    private bool faceUp;

    public Card data;

    // Start is called before the first frame update
    void Start()
    {
        allowedGameStates = new HashSet<Current_UI_State>() { Current_UI_State.SelectionWindow };
    }

    private void OnEnable()
    {
        Total_UI.uiStateChanged += onUIstateUpdate;
    }

    private void OnDisable()
    {
        Total_UI.uiStateChanged -= onUIstateUpdate;
    }

    public void initialize(Card baseCard, bool faceUp)
    {
        data = baseCard;

        if(baseCard.cardType == CardType.CLUE)
        {
            ClueCard cc = baseCard as ClueCard;
            textField1.text = ((int) cc.cpdType).ToString();
            textField2.text = cc.cpdType.ToString();
            textField3.text = cc.category;
        } else if(baseCard.cardType == CardType.ACTION)
        {
            ActionCard ac = baseCard as ActionCard;
            textField1.text = "X";
            textField2.text = ac.actionCardType.ToString();
        }
        this.faceUp = faceUp;

        activeUI = true;
    }


    // When selected, if it is face down, reveal to other players
    public void Reveal()
    {
        if (!faceUp)
        {
            transform.parent.localRotation = Quaternion.Euler(0, 0, 0);
            faceUp = true;
        }
    }


    public void OnMouseEnter()
    {
        if(activeUI)
        {
            selectedSpr.gameObject.SetActive(true);
        } else
        {
            Debug.Log("Failed - the state is " + Total_UI.uiState);
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
        if(activeUI)
        {
            // Pass along
            Total_UI.instance.changeUIState(Current_UI_State.GenTransition);
            SelectionWindow.instance.madeChoice(this);
        }
    }
}
