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

    public Agent playerAgent;
    public int totalRotations;
    private int currTurn;  // from 0-num agents

    public List<Card> generalDeck;


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

        playerAgent = new PlayerAgent();
        agentsInOrder.Add(playerAgent);

        CPUAgent cpuAgent1 = new CPUAgent(1, "Hazel");
        agentsInOrder.Add(cpuAgent1);

        CPUAgent cpuAgent2 = new CPUAgent(1, "Winter");
        agentsInOrder.Add(cpuAgent1);
    }

    private void OnEnable()
    {
        RosterGen.rosterCreationDone += generateDeck;
        PlayerAgent.playerTurnOver += nextInLine;
        CPUAgent.cpuTurnOver += nextInLine;
    }

    private void OnDisable()
    {
        RosterGen.rosterCreationDone -= generateDeck;
        PlayerAgent.playerTurnOver -= nextInLine;
    }

    // Generate the deck once you know what it should contain
    private void generateDeck(Roster _)
    {
        generalDeck = new List<Card>();

        /*generalDeck.Add(new ActionCard(ActionCardType.REPEAT));
        generalDeck.Add(new ActionCard(ActionCardType.SKIP));
        generalDeck.Add(new ActionCard(ActionCardType.REVEAL));
        generalDeck.Add(new ActionCard(ActionCardType.STEAL));
        generalDeck.Add(new ActionCard(ActionCardType.DRAW_2));
        generalDeck.Add(new ActionCard(ActionCardType.EXTRA_INVENTORY));
        generalDeck.Add(new ActionCard(ActionCardType.EXTRA_ASK_AROUND));
        generalDeck.Add(new ActionCard(ActionCardType.ORACLE));*/

        foreach (CPD cpd in Roster.cpdConstrainables)
        {
            foreach(string cat in cpd.categories)
            {
                generalDeck.Add(new ClueCard(cpd.cpdType, cat, false));
            }
        }

        // Shuffle in place
        generalDeck = Utility.Shuffle<Card>(generalDeck);

        roundSetup();
    }

    // Give a card from the deck to an agent
    public void acquireCard(Agent agent)
    {
        Card toGive = generalDeck[0];
        generalDeck.RemoveAt(0);

        agent.acquireCard(toGive);
    }

    // Give a card specifically to the player
    public void playerAcquireCard()
    {
        acquireCard(playerAgent);
    }


    public void roundSetup()
    {
        // Generate and Shuffle general deck

        currTurn = 0;
        agentsInOrder[0].markAsReady();
    }


    // Proceed to next agent for their turn
    public void nextInLine()
    {
        currTurn = (currTurn + 1) % agentsInOrder.Count;

        agentsInOrder[currTurn].markAsReady();
    }
}
