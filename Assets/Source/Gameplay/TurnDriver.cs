using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turn Driver: Manages a normal turn of gameplay
/// </summary>
public class TurnDriver : MonoBehaviour
{
    public static TurnDriver instance;

    public List<Agent> agentsInOrder = new List<Agent>();
    public Dictionary<int, Agent> agentIDmappings = new Dictionary<int, Agent>();

    public Roster currentRoster;
    public Agent playerAgent;
    public int totalRotations;
    private int currTurn;  // from 0-num agents

    public List<ClueCard> clueCardDeck;
    public List<ActionCard> actionCardDeck;


    // TODO make doable for many
    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        } else
        {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        RosterGen.rosterCreationDone += onRosterCreation;
        PlayerAgent.playerTurnOver += nextInLine;
        CPUAgent.cpuTurnOver += nextInLine;
    }

    private void OnDisable()
    {
        RosterGen.rosterCreationDone -= onRosterCreation;
        PlayerAgent.playerTurnOver -= nextInLine;
    }

    private void onRosterCreation(Roster rost)
    {
        currentRoster = rost;
        generatePlayers();
        Total_UI.instance.initializeUI(agentsInOrder, rost);
        generateDeck(rost);
    }

    // Generate the deck once you know what it should contain
    private void generateDeck(Roster rost)
    {
        clueCardDeck = new List<ClueCard>();
        actionCardDeck = new List<ActionCard>();

        // Add to deck: All action card types
        actionCardDeck.Add(new ActionCard(ActionCardType.CENSOR));
        actionCardDeck.Add(new ActionCard(ActionCardType.CENSOR));
        actionCardDeck.Add(new ActionCard(ActionCardType.CENSOR));

        actionCardDeck.Add(new ActionCard(ActionCardType.INTERN));
        actionCardDeck.Add(new ActionCard(ActionCardType.INTERN));
        actionCardDeck.Add(new ActionCard(ActionCardType.INTERN));


        // Get properties of target
        List<CPD_Variant> targetData = rost.getTargetAsCPDs();
        Dictionary<CPD_Type, string> targetProperties = new Dictionary<CPD_Type, string>();
        foreach(CPD_Variant cpdVar in targetData)
        {
            targetProperties.Add(cpdVar.cpdType, cpdVar.category);
        }

        // Add to deck: All CPD cards except for the ones matching the target
        foreach (CPD cpd in Roster.cpdConstrainables)
        {
            foreach(string cat in cpd.categories)
            {
                if(cat != targetProperties[cpd.cpdType])
                {
                    clueCardDeck.Add(new ClueCard(cpd.cpdType, cat, false));
                }
            }
        }

        // Shuffle in place
        clueCardDeck = Utility.Shuffle<ClueCard>(clueCardDeck);

        StartCoroutine(roundSetup());
    }

    private void generatePlayers()
    {
        playerAgent = new PlayerAgent();
        agentsInOrder.Add(playerAgent);

        CPUAgent cpuAgent1 = new CPUAgent(1, "Hazel");
        agentsInOrder.Add(cpuAgent1);

        CPUAgent cpuAgent2 = new CPUAgent(2, "Winter");
        agentsInOrder.Add(cpuAgent2);
    }






    // TODO: Animations and stuff
    private IEnumerator roundSetup()
    {
        yield return new WaitForSeconds(3);

        // Distribute the cards to all players.
        int agentIndex = 0;
        for(int i = 0; i < clueCardDeck.Count; i++)
        {
            agentsInOrder[agentIndex].startingDealtCard(clueCardDeck[i]);
            agentIndex = (agentIndex + 1) % agentsInOrder.Count;
            yield return null;
        }

        currTurn = 0;
        agentsInOrder[0].markAsReady();
    }


    // Proceed to next agent for their turn
    public void nextInLine()
    {
        currTurn = (currTurn + 1) % agentsInOrder.Count;

        agentsInOrder[currTurn].markAsReady();
    }

    public void dealActionCard()
    {
        if(actionCardDeck.Count > 0)
        {
            ActionCard next = actionCardDeck[0];
            actionCardDeck.RemoveAt(0);

            agentsInOrder[currTurn].acquireCard(next);
        } else
        {
            Debug.LogWarning("No action cards left to deal!");
        }
        
    }
}
