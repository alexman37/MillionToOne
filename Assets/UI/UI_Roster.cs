using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Roster : MonoBehaviour
{
    public const int CHARACTERS_TO_SHOW = 20;

    public static UI_Roster instance;

    private Roster roster;

    //UI components
    public Image rosterWindow;
    public Image characterCardTemplate;
    public TextMeshProUGUI suspectsRemaining;

    private GameObject container;
    private GameObject[] createdCards;
    [SerializeField] private GameObject rosterFormContainer;

    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        createContainer();
        rosterWindow.gameObject.SetActive(false);
        RosterGen.rosterCreationDone += setRoster;
        createdCards = new GameObject[CHARACTERS_TO_SHOW];
    }

    private void OnEnable()
    {
        Roster.constrainedResult += updateRosterCount;
        FormButton.updatedConstraint += handleUpdatedConstraint;
        FormButton.reinitializeConstraints += handleDeconfirmed;
        //Roster.constrainedResult += regenerateCharCards;
        Roster.rosterReady += rosterFormCreation;
    }

    private void OnDisable()
    {
        Roster.constrainedResult -= updateRosterCount;
        FormButton.updatedConstraint -= handleUpdatedConstraint;
        FormButton.reinitializeConstraints -= handleDeconfirmed;
        //Roster.constrainedResult -= regenerateCharCards;
        Roster.rosterReady -= rosterFormCreation;
    }

    void createContainer()
    {
        container = new GameObject();
        container.transform.SetParent(transform.parent.parent);
        container.transform.position = new Vector3(100, 100, 0);
    }

    void setRoster(Roster rost)
    {
        roster = rost;
    }

    public void toggleRosterWindow()
    {
        bool newVal = !rosterWindow.gameObject.activeInHierarchy;
        if (newVal == true) generateAllCharCards();
        else
        {
            Destroy(container);
            createContainer();
        }

        rosterWindow.gameObject.SetActive(newVal);
    }

    public void updateRosterCount(int newCount)
    {
        Debug.Log("Updated roster?");
        suspectsRemaining.text = newCount.ToString() + " Suspects Remaining";
    }

    public void generateAllCharCards()
    {
        int entriesPerRow = 10;
        float startingX = characterCardTemplate.rectTransform.position.x;
        float startingY = characterCardTemplate.rectTransform.position.y;
        float cardWidth = characterCardTemplate.rectTransform.rect.width;
        float cardHeight = characterCardTemplate.rectTransform.rect.height;
        float cardOffsetW = cardWidth / 10f;
        float cardOffsetH = cardHeight / 10f;


        for (int i = 0; i < CHARACTERS_TO_SHOW; i++)
        {
            Character c = roster.roster[i];

            //instantiate card in correct position
            Image newCard = GameObject.Instantiate(characterCardTemplate);
            newCard.transform.SetParent(container.transform);
            newCard.rectTransform.position = new Vector3(
                startingX + Mathf.Floor(i % entriesPerRow) * (cardWidth + cardOffsetW), 
                startingY - Mathf.Floor(i / entriesPerRow) * (cardHeight + cardOffsetH), 0);
            newCard.gameObject.SetActive(true);

            roster.rosterSprites[i].name = i.ToString();
            // TODO would it be better as a sprite renderer???
            newCard.transform.GetChild(0).GetComponent<Image>().sprite = roster.rosterSprites[i];

            //set portrait and name
            newCard.GetComponentInChildren<TextMeshProUGUI>().text = c.getDisplayName(true);

            createdCards[i] = newCard.gameObject;
        }
    }

    // TODO - fancier animations for this - one day.
    public void regenerateCharCards(int newNumber)
    {
        if(roster != null)
        {
            int numPortraits = Mathf.Min(newNumber, CHARACTERS_TO_SHOW);
            for (int i = 0; i < numPortraits; i++)
            {
                createdCards[i].SetActive(true);
                Character c = roster.roster[i];

                //instantiate card in correct position
                Image newCard = createdCards[i].GetComponent<Image>();

                roster.rosterSprites[i].name = i.ToString();
                // TODO would it be better as a sprite renderer???
                newCard.transform.GetChild(0).GetComponent<Image>().sprite = roster.rosterSprites[i];

                //set portrait and name
                newCard.GetComponentInChildren<TextMeshProUGUI>().text = c.getDisplayName(true);
            }
            for(int i = numPortraits; i < CHARACTERS_TO_SHOW; i++)
            {
                createdCards[i].SetActive(false);
            }
        }
    }

    private void rosterFormCreation()
    {
        rosterFormContainer.GetComponent<RosterForm>().enabled = true;
    }

    private void handleUpdatedConstraint(CPD_Type cpdType, string value, FormButtonState newState)
    {
        Debug.Log("Updated constraints?");
        switch (newState)
        {
            case FormButtonState.Unknown:
                roster.rosterConstraints.removeConstraint(cpdType, value);
                break;
            case FormButtonState.Eliminated:
                roster.rosterConstraints.addConstraint(cpdType, value);
                break;
            case FormButtonState.Confirmed:
                roster.rosterConstraints.onlyConstraint(cpdType, value);
                break;
        }
        Debug.Log(roster.rosterConstraints.allCurrentConstraints[cpdType].Count);
        roster.redrawRosterVis();
    }

    private void handleDeconfirmed(CPD_Type cpdType, List<string> exclude)
    {
        roster.reInitializeVariants(cpdType, exclude);
    }
}
