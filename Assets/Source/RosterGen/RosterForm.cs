using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// RosterForm is the controller for the "sorting menu" the player uses to narrow down the roster to their target.
/// </summary>
public class RosterForm : MonoBehaviour
{
    public static RosterForm instance;
    RectTransform self;

    [SerializeField] GameObject formButtonGroupTemplate; // use this prefab to create formField groups.
    CPD[] formFields; // the list of form fields. There should be one for each CONSTRAINABLE CPD.
                      // (other CPDs can exist and be irrelevant in terms of sorting - such as face.)

    // Related to asking around
    [SerializeField] private TextMeshProUGUI formTitle;
    [SerializeField] private GameObject askAroundCommands;
    private List<CPD_Category> askingFor = new List<CPD_Category>();
    private int agentToAsk;
    public static event Action completedAskAround;

    private float nextFormGroupOffset = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        self = this.GetComponent<RectTransform>();

        // Since this depends on the roster being created first, we actually want to instantiate a RosterForm object
        // within Roster, once we know it's at least finished gathering all the CPD lists together.
        formFields = Roster.cpdConstrainables.ToArray();
        createFormButtonGroups();
    }

    private void OnEnable()
    {
        AgentDisplay.selectedAgent += StartedAskingAround;
        AgentDisplay.deselectedAgent += StoppedAskingAround;
        FormButton.addToAskAroundList += AddToAskFor;
        FormButton.removeFromAskAroundList += AddToAskFor;
    }

    private void OnDisable()
    {
        AgentDisplay.selectedAgent -= StartedAskingAround;
        AgentDisplay.deselectedAgent -= StoppedAskingAround;
        FormButton.addToAskAroundList -= AddToAskFor;
        FormButton.removeFromAskAroundList -= AddToAskFor;
    }

    /// <summary>
    /// For each constrainable CPD, build the sorting menu.
    /// </summary>
    public void createFormButtonGroups()
    {
        nextFormGroupOffset = 0;

        foreach(CPD cpd in formFields)
        {
            createWithParameters(cpd.cpdType, cpd.categories);
        }

        self.sizeDelta = new Vector2(self.rect.width, nextFormGroupOffset);
    }

    /// <summary>
    /// Build the sorting menu for a single CPD
    /// </summary>
    private void createWithParameters(CPD_Type cpdType, List<string> cpdCategories)
    {
        GameObject next = GameObject.Instantiate(formButtonGroupTemplate, this.transform);
        next.name = cpdType.ToString();

        FormButtonGroup formGroup = next.GetComponent<FormButtonGroup>();

        formGroup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = cpdType.ToString().ToUpper();

        // Offset every form field grouping by a constant amount of pixels
        float totalHeight = formGroup.buildFormButtonGroup(cpdType, cpdCategories, -nextFormGroupOffset) + 30;
        RectTransform container = next.GetComponent<Image>().rectTransform;
        container.sizeDelta = new Vector2(container.rect.width, totalHeight + 10);
        nextFormGroupOffset += totalHeight + 20;
    }


    private void StartedAskingAround(int id)
    {
        askAroundCommands.SetActive(true);
        formTitle.text = "ASKING " + id;
    }

    public void StoppedAskingAround()
    {
        askAroundCommands.SetActive(false);
        formTitle.text = "CHARACTER SHEET";
        completedAskAround.Invoke();
    }

    private void AddToAskFor(CPD_Category cat)
    {
        askingFor.Add(cat);
    }

    private void RemoveFromAskFor(CPD_Category cat)
    {
        askingFor.Remove(cat);
    }

    public void AskAround()
    {
        // TODO: Ask for this agent with these properties
        Debug.LogWarning("Not implemented asking around yet");
        StoppedAskingAround();
    }
}
