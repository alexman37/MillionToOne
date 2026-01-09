using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Roster: This handles drawing character cards in the roster menu.
/// </summary>
public class UI_Roster : MonoBehaviour
{
    // How many characters should we display at a time?
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

        createdCards = new GameObject[CHARACTERS_TO_SHOW];
    }

    private void OnEnable()
    {
        RosterGen.rosterCreationDone += setRoster;
        Roster.constrainedResult += updateRosterCount;
        FormButton.updatedConstraint += handleUpdatedConstraint;
        FormButton.reinitializeConstraints += handleDeconfirmed;
        Roster.rosterReady += rosterFormCreation;
    }

    private void OnDisable()
    {
        RosterGen.rosterCreationDone -= setRoster;
        Roster.constrainedResult -= updateRosterCount;
        FormButton.updatedConstraint -= handleUpdatedConstraint;
        FormButton.reinitializeConstraints -= handleDeconfirmed;
        Roster.rosterReady -= rosterFormCreation;
    }

    void createContainer()
    {
        Destroy(container);

        container = new GameObject();
        container.name = "RosterContainer";
        container.AddComponent<RectTransform>();
        RectTransform rt = container.GetComponent<RectTransform>();

        container.transform.SetParent(transform);

        Debug.Log(container.GetComponent<RectTransform>().pivot);
        rt.pivot = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.anchorMin = new Vector2(0, 1);
        Debug.Log(container.GetComponent<RectTransform>().pivot);
        rt.anchoredPosition = new Vector2(0, 0);
    }

    void setRoster(Roster rost)
    {
        roster = rost;

        // assume this also means we want to generate cards
        generateAllCharCards();
    }

    /// <summary>
    /// Show or hide the roster window
    /// </summary>
    public void toggleRosterWindow()
    {
        bool newVal = !rosterWindow.gameObject.activeInHierarchy;
        if (newVal == true) generateAllCharCards();
        else
        {
            Destroy(container);
            container = null;
            createContainer();
        }

        rosterWindow.gameObject.SetActive(newVal);
    }

    /// <summary>
    /// Change the display at the top of the roster to show a new number
    /// </summary>
    public void updateRosterCount(int newCount)
    {
        suspectsRemaining.text = newCount.ToString() + " Suspects Remaining";
    }

    /// <summary>
    /// Generate all character cards for the first time
    /// </summary>
    public void generateAllCharCards()
    {
        createContainer();

        int entriesPerRow = 10;
        float startingX = characterCardTemplate.rectTransform.position.x;
        float startingY = characterCardTemplate.rectTransform.position.y - 100;
        float cardWidth = characterCardTemplate.rectTransform.rect.width;
        float cardHeight = characterCardTemplate.rectTransform.rect.height;
        float cardOffsetW = cardWidth / 10f;
        float cardOffsetH = cardHeight / 10f;


        for (int i = 0; i < CHARACTERS_TO_SHOW; i++)
        {
            Debug.Log(roster.shownRoster.Count);
            Character c = roster.shownRoster[i];

            //instantiate card in correct position
            Image newCard = GameObject.Instantiate(characterCardTemplate);
            newCard.transform.SetParent(container.transform);
            newCard.rectTransform.localPosition = new Vector3(
                startingX + Mathf.Floor(i % entriesPerRow) * (cardWidth + cardOffsetW), 
                startingY - Mathf.Floor(i / entriesPerRow) * (cardHeight + cardOffsetH), 0);
            newCard.gameObject.SetActive(true);
            Debug.Log("POS " + newCard.rectTransform.position);

            roster.shownRosterSprites[i].name = i.ToString();
            // TODO would it be better as a sprite renderer???
            newCard.transform.GetChild(0).GetComponent<Image>().sprite = roster.shownRosterSprites[i];

            //set portrait and name
            newCard.GetComponentInChildren<TextMeshProUGUI>().text = c.getDisplayName(true) + "\n (" + roster.shownRoster[i].simulatedId + ")";

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
                Character c = roster.shownRoster[i];

                //instantiate card in correct position
                Image newCard = createdCards[i].GetComponent<Image>();

                roster.shownRosterSprites[i].name = i.ToString();
                // TODO would it be better as a sprite renderer???
                newCard.transform.GetChild(0).GetComponent<Image>().sprite = roster.shownRosterSprites[i];

                //set portrait and name
                newCard.GetComponentInChildren<TextMeshProUGUI>().text = c.getDisplayName(true) + "\n (" + roster.shownRoster[i].simulatedId + ")";
            }
            for(int i = numPortraits; i < CHARACTERS_TO_SHOW; i++)
            {
                createdCards[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// When roster is finished defining all CPDs, we create the RosterForm here
    /// </summary>
    private void rosterFormCreation()
    {
        rosterFormContainer.GetComponent<RosterForm>().enabled = true;
    }

    /// <summary>
    /// When a new constraint is updated from the roster form, we need to relay it to the Roster object
    /// </summary>
    private void handleUpdatedConstraint(CPD_Type cpdType, string value, FormButtonState newState)
    {
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

        roster.redrawRosterVis();
    }

    /// <summary>
    /// When a roster object is "deconfirmed" we'll reset its constraints list (and then repopulate it with old ones if needed.)
    /// </summary>
    private void handleDeconfirmed(CPD_Type cpdType, List<string> exclude)
    {
        roster.reInitializeVariants(cpdType, exclude);
    }
}
