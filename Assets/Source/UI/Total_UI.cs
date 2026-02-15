using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// For managing UI things at the highest level.
/// </summary>
public class Total_UI : MonoBehaviour
{
    public static Total_UI instance;

    public Inventory inventory;
    public Image deckStation;
    public Image suspectView;
    public Image clueForm;
    public Image numSuspects;
    public Playerbase playerbase;
    

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void initializeUI(List<Agent> agentsInOrder, Roster rost)
    {
        playerbase.initialize(agentsInOrder, rost.simulatedTotalRosterSize);
    }
}
