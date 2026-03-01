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

    public Roster currentRoster;
    public Agent playerAgent;
    public int totalRotations;
    private int currTurn;  // from 0-num agents

    public List<ClueCard> clueCardDeck;
    public List<ActionCard> actionCardDeck;
    public List<GoldCard> goldCardDeck;


    // For action / reaction chains. What card is trying to be played?
    public PersonCard queuedCard;


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

        SelectionWindow.playerReacts += executeActionCard;
        CPUAgent.cpuReacts += executeActionCard;
    }

    private void OnDisable()
    {
        RosterGen.rosterCreationDone -= onRosterCreation;
        PlayerAgent.playerTurnOver -= nextInLine;

        SelectionWindow.playerReacts -= executeActionCard;
        CPUAgent.cpuReacts -= executeActionCard;
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
        goldCardDeck = new List<GoldCard>();

        // Add to deck: All action card types

        goldCardDeck.Add(new GoldCard(GoldCardType.THIEF));
        goldCardDeck.Add(new GoldCard(GoldCardType.THIEF));

        goldCardDeck.Add(new GoldCard(GoldCardType.INSIDER));
        goldCardDeck.Add(new GoldCard(GoldCardType.INSIDER));
        goldCardDeck.Add(new GoldCard(GoldCardType.INSIDER));
        goldCardDeck.Add(new GoldCard(GoldCardType.INSIDER));
        goldCardDeck.Add(new GoldCard(GoldCardType.INSIDER));
        goldCardDeck.Add(new GoldCard(GoldCardType.INSIDER));

        goldCardDeck.Add(new GoldCard(GoldCardType.HACKER));
        goldCardDeck.Add(new GoldCard(GoldCardType.HACKER));

        goldCardDeck.Add(new GoldCard(GoldCardType.ASSASSAIN));
        goldCardDeck.Add(new GoldCard(GoldCardType.ASSASSAIN));

        goldCardDeck.Add(new GoldCard(GoldCardType.ESCORT));
        goldCardDeck.Add(new GoldCard(GoldCardType.ESCORT));

        actionCardDeck.Add(new ActionCard(ActionCardType.INTERN));

        actionCardDeck.Add(new ActionCard(ActionCardType.ENFORCER));
        actionCardDeck.Add(new ActionCard(ActionCardType.INTERN));
        actionCardDeck.Add(new ActionCard(ActionCardType.LAWYER));
        actionCardDeck.Add(new ActionCard(ActionCardType.INTERN));
        actionCardDeck.Add(new ActionCard(ActionCardType.ENFORCER));

        
        actionCardDeck.Add(new ActionCard(ActionCardType.LAWYER));
        actionCardDeck.Add(new ActionCard(ActionCardType.LAWYER));

        actionCardDeck.Add(new ActionCard(ActionCardType.ANALYST));
        actionCardDeck.Add(new ActionCard(ActionCardType.ANALYST));
        actionCardDeck.Add(new ActionCard(ActionCardType.ANALYST));

        actionCardDeck.Add(new ActionCard(ActionCardType.SIDEKICK));
        actionCardDeck.Add(new ActionCard(ActionCardType.SIDEKICK));
        actionCardDeck.Add(new ActionCard(ActionCardType.SIDEKICK));

        actionCardDeck.Add(new ActionCard(ActionCardType.CENSOR));
        actionCardDeck.Add(new ActionCard(ActionCardType.CENSOR));
        actionCardDeck.Add(new ActionCard(ActionCardType.CENSOR));


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
        Debug.Log("Marking player " + currTurn + " as ready ");
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

    public void dealGoldCard()
    {
        if (goldCardDeck.Count > 0)
        {
            GoldCard next = goldCardDeck[0];
            goldCardDeck.RemoveAt(0);

            agentsInOrder[currTurn].acquireCard(next);
        }
        else
        {
            Debug.LogWarning("No action cards left to deal!");
        }
    }

    public Card getNextCardInLine(CardType cardType, bool andRemove)
    {
        Card toGive;
        switch(cardType)
        {
            case CardType.CLUE:
                toGive = clueCardDeck[0];
                if(andRemove) clueCardDeck.RemoveAt(0);
                break;
            case CardType.ACTION:
                toGive = actionCardDeck[0];
                if (andRemove) actionCardDeck.RemoveAt(0);
                break;
            default:
                toGive = goldCardDeck[0];
                if (andRemove) goldCardDeck.RemoveAt(0);
                break;
        }
        return toGive;
    }

    public void executeActionCard(Agent playingAgent, Agent targetAgent, ReactionVerdict verdict) { 

        switch(verdict)
        {
            // The player couldn't do anything about it, so continue as normal
            case ReactionVerdict.ALLOW:
                if (playingAgent.id == 0)     ActionHandler_PA.handleFinalPlayedAction(queuedCard, targetAgent);
                else                          ActionHandler_CPU.handleFinalPlayedAction(queuedCard, targetAgent);
                break;
            case ReactionVerdict.BLOCK:
                Debug.Log("The action was blocked by a bodyguard!");
                break;
            case ReactionVerdict.REVERSE:
                queuedCard.owner = targetAgent;
                if (targetAgent.id == 0)   ActionHandler_PA.handleFinalPlayedAction(queuedCard, playingAgent);
                else                       ActionHandler_CPU.handleFinalPlayedAction(queuedCard, playingAgent);
                break;
        }
    }

    public void giveReward(int toId, TargetCPDGuessReward rewardType)
    {
        if (rewardType == TargetCPDGuessReward.ActionCard) agentsInOrder[toId].acquireCard(getNextCardInLine(CardType.ACTION, true));
        else if (rewardType == TargetCPDGuessReward.GoldCard) agentsInOrder[toId].acquireCard(getNextCardInLine(CardType.GOLD, true));
    }
}
