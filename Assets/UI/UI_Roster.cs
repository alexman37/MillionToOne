using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Roster : MonoBehaviour
{
    public const int CHARACTERS_TO_SHOW = 20;

    private Roster roster;

    //UI components
    public Image rosterWindow;
    public Image characterCardTemplate;
    public TextMeshProUGUI suspectsRemaining;

    private GameObject container;
    [SerializeField] private GameObject rosterFormContainer;

    void Start()
    {
        createContainer();
        rosterWindow.gameObject.SetActive(false);
        RosterGen.rosterCreationDone += setRoster;
    }

    private void OnEnable()
    {
        Roster.constrainedResult += updateRosterCount;
        FormButton.updatedConstraint += handleUpdatedConstraint;
        FormButton.reinitializeConstraints += handleDeconfirmed;
        Roster.rosterReady += rosterFormCreation;
    }

    private void OnDisable()
    {
        Roster.constrainedResult -= updateRosterCount;
        FormButton.updatedConstraint -= handleUpdatedConstraint;
        FormButton.reinitializeConstraints -= handleDeconfirmed;
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
        }
    }

    private void rosterFormCreation()
    {
        rosterFormContainer.GetComponent<RosterForm>().enabled = true;
    }

    private void handleUpdatedConstraint(string onField, string value, FormButtonState newState)
    {
        switch (newState)
        {
            case FormButtonState.Unknown:
                roster.addConstraint(onField, value);
                break;
            case FormButtonState.Eliminated:
                roster.removeConstraint(onField, value);
                break;
            case FormButtonState.Confirmed:
                roster.onlyConstraint(onField, value);
                break;
        }
    }

    private void handleDeconfirmed(string group, List<string> exclude)
    {
        roster.reInitializeVariants(group, exclude);
    }
}
