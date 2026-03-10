using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class PopupCanvas : MonoBehaviour
{
    public static PopupCanvas instance;

    // Guess Target Properties
    public Canvas popupCanvas;
    public GameObject targetPropertyGuess;

    // You've been asked to show some cards
    public GameObject askedAbout;
    [SerializeField] TextMeshProUGUI askedAboutTitle;
    [SerializeField] GameObject displayCardTemplate;

    // You get the result back of what cards were shown
    public GameObject askAroundResult;
    [SerializeField] TextMeshProUGUI askAroundTitle;

    private List<TargetCharGuess> targetPropertyEntries = new List<TargetCharGuess>();
    private List<GameObject> displayCardObjects = new List<GameObject>();


    public static event Action popupAskAroundResponseReceived = () => { };


    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(this);
    }

    // Player guesses a characteristic of the target
    public void popup_targetPropertyGuess(CPD_Type cpdType)
    {
        GameObject targetPropertyEntry = targetPropertyGuess.transform.GetChild(0).gameObject;
        List<string> cats = Roster.cpdByType[cpdType].categories;

        for (int i = 0; i < cats.Count; i++)
        {
            string cat = cats[i];

            // Instantiate buttons for guessing
            GameObject next = GameObject.Instantiate(targetPropertyEntry, targetPropertyGuess.transform);
            next.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, -60 * i);

            TargetCharGuess tcg = next.GetComponent<TargetCharGuess>();
            tcg.initialize(cpdType, cat);
            targetPropertyEntries.Add(tcg);
            next.SetActive(true);

            popupCanvas.enabled = true;
            targetPropertyGuess.SetActive(true);
        }
    }

    public void popup_targetPropertyClear()
    {
        foreach (TargetCharGuess go in targetPropertyEntries)
        {
            Destroy(go.gameObject);
        }
        targetPropertyEntries.Clear();

        popupCanvas.enabled = false;
        targetPropertyGuess.SetActive(false);
    }

    public void popup_askedAbout(Agent askedBy, List<(CPD_Type cpd, string cat)> inquiry)
    {
        int count = 0;
        askedAbout.SetActive(true);

        int i = 0;
        foreach((CPD_Type cpd, string cat) query in inquiry)
        {
            GameObject nextDisplayCard = GameObject.Instantiate(displayCardTemplate, askedAbout.transform);
            DisplayCard dc = nextDisplayCard.GetComponent<DisplayCard>();
            nextDisplayCard.transform.localPosition = new Vector3(-560 + 220 * i, -730, -1);

            dc.initWith(((int)query.cpd).ToString(), query.cpd.ToString(), query.cat);
            count++;
        }

        askedAboutTitle.text = "Agent " + askedBy.agentName + " guessed " + count + " of your cards!";
    }

    public void popup_askedAboutClear()
    {
        foreach(GameObject go in displayCardObjects)
        {
            Destroy(go);
        }
        displayCardObjects.Clear();
        askedAbout.SetActive(false);
    }

    public void popup_askAroundResult(List<(CPD_Type cpd, string cat)> shown)
    {
        int count = 0;
        askAroundResult.SetActive(true);

        foreach ((CPD_Type cpd, string cat) query in shown)
        {
            GameObject nextDisplayCard = GameObject.Instantiate(displayCardTemplate, askAroundResult.transform);
            DisplayCard dc = nextDisplayCard.GetComponent<DisplayCard>();
            nextDisplayCard.transform.localPosition = new Vector3(-560 + 220 * count, -730, -1);

            dc.initWith(((int)query.cpd).ToString(), query.cpd.ToString(), query.cat);
            count++;
        }

        askAroundTitle.text = "Here's what they had: " + count + " cards";
    }

    public void aaResponse_showAllCards()
    {
        popup_askedAboutClear();
        PlayerAgent.instance.aaRespond_Show();
    }

    public void popup_askAroundResultClear(bool playerAsked)
    {
        foreach (GameObject go in displayCardObjects)
        {
            Destroy(go);
        }
        displayCardObjects.Clear();
        askAroundResult.SetActive(false);

        // Dispatch this action only when the player is asking another agent for info
        if (playerAsked)
            popupAskAroundResponseReceived.Invoke();
    }

    public void popup_askAroundVague(List<(CPD_Type cpd, string cat)> inquiry, int numCorrect)
    {
        int count = 0;
        askAroundResult.SetActive(true);

        foreach ((CPD_Type cpd, string cat) query in inquiry)
        {
            GameObject nextDisplayCard = GameObject.Instantiate(displayCardTemplate, askAroundResult.transform);
            DisplayCard dc = nextDisplayCard.GetComponent<DisplayCard>();
            nextDisplayCard.transform.localPosition = new Vector3(-560 + 220 * count, -730, -1);

            dc.initWith(((int)query.cpd).ToString(), query.cpd.ToString(), query.cat);
            count++;
        }

        askAroundTitle.text = "Of this inquiry, the asked agent had " + numCorrect + " cards.";
    }

    // Vague uses same clear as result
}
