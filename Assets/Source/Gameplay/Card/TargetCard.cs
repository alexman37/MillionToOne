using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class TargetCard : MonoBehaviour
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
        
    }

    private void OnEnable()
    {
        TargetCharGuess.playerGuessesTargetProperty += RevealToAll;
    }

    private void OnDisable()
    {
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
        if(!revealed)
        {
            selectedSpr.gameObject.SetActive(true);
        }
    }

    public void OnMouseExit()
    {
        selectedSpr.gameObject.SetActive(false);
    }

    public void OnMouseDown()
    {
        if(!revealed)
        {
            PopupCanvas.instance.popup_targetPropertyGuess(cpdType);
        }
    }
}
