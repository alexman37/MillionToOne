using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Appears when the player uses an Action card that requires selecting a card from someone's hand.
/// </summary>
public class SelectionWindow : ConditionalUI
{
    public static SelectionWindow instance;

    [SerializeField] GameObject container;
    [SerializeField] GameObject selectionCardTemplate_clue;
    [SerializeField] GameObject selectionCardTemplate_action;
    [SerializeField] GameObject selectionCardTemplate_gold;

    List<SelectionCard> cardsInSelectionWindow = new List<SelectionCard>();

    Vector3 defaultPos = new Vector3(-10, 21, 0);
    int maxInRow = 5;
    int allowedSelections = 1;
    public SelectionCardOutcome currentOutcome;

    // Don't need these unless we have to wait for an agent selection
    private int as_whatKinds;
    private bool as_cardsFaceUp;

    // For converting the intern card
    private PersonCard intern;

    // For reactions
    private PersonCard actionCardAtStake;


    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        allowedGameStates = new HashSet<Current_UI_State>() { Current_UI_State.SelectionWindow, Current_UI_State.ReactionWindow };
    }

    private void OnEnable()
    {
        AgentDisplay.selectedAgent_AS += displayAfterAgentSelect;
    }

    private void OnDisable()
    {
        AgentDisplay.selectedAgent_AS -= displayAfterAgentSelect;
    }


    public void prepareForDisplay(SelectionCardOutcome outcome, int times, int whatKinds, bool cardsFaceUp)
    {
        currentOutcome = outcome;
        allowedSelections = times;

        as_whatKinds = whatKinds;
        as_cardsFaceUp = cardsFaceUp;
    }

    private void displayAfterAgentSelect(int agentId, AgentSelectReason selectReason)
    {
        if(selectReason == AgentSelectReason.CardSelect)
        {
            displaySelection(currentOutcome, allowedSelections, TurnDriver.instance.agentsInOrder[agentId], as_whatKinds, as_cardsFaceUp);
        }
    }

    /// <summary>
    /// whatKinds: 0 = clue cards only, 1 = action cards only, 2 = all
    /// </summary>
    public void displaySelection(SelectionCardOutcome outcome, int times, Agent agent, int whatKinds, bool cardsFaceUp)
    {
        Debug.Log("Display selection...");
        Total_UI.instance.changeUIState(Current_UI_State.SelectionWindow);
        container.SetActive(true);

        currentOutcome = outcome;
        allowedSelections = times;

        List<Card> cardsToGet = new List<Card>();
        if (whatKinds != 1) cardsToGet.AddRange(agent.inventory);
        if (whatKinds != 0) cardsToGet.AddRange(agent.recruits);

        int count = 0;
        foreach (Card card in cardsToGet)
        {
            GameObject template;
            switch (card.cardType)
            {
                case CardType.CLUE: template = selectionCardTemplate_clue; break;
                case CardType.ACTION: template = selectionCardTemplate_action; break;
                default: template = selectionCardTemplate_gold; break;
            }
            GameObject go = GameObject.Instantiate(template, this.transform);

            SelectionCard sCard = go.GetComponentInChildren<SelectionCard>();
            sCard.initialize(card, cardsFaceUp);

            go.transform.localPosition = defaultPos + new Vector3(3 * (count % maxInRow), -5 * (count / maxInRow), 0);
            go.transform.localRotation = Quaternion.Euler(0, cardsFaceUp ? 0 : 180, 0);

            cardsInSelectionWindow.Add(sCard);

            count++;
        }
    }

    public void madeChoice(SelectionCard chosen)
    {
        StartCoroutine(madeChoiceCo(chosen));
    }

    private IEnumerator madeChoiceCo(SelectionCard chosen)
    {
        Card data = chosen.data;
        int waitSec = 0;

        // Do something with the selection
        switch (instance.currentOutcome)
        {
            case SelectionCardOutcome.REDACTION:
                (data as ClueCard).redact();
                PlayerAgent.instance.playCard(data);
                break;
            case SelectionCardOutcome.VIEW:
                chosen.Reveal();
                if(data.cardType == CardType.CLUE)
                {
                    PlayerAgent.instance.onClueCardDeclassified(data as ClueCard);
                }
                waitSec = 2;
                break;
            case SelectionCardOutcome.DECLASS:
                // TODO no reward for this
                (data as ClueCard).owner.playCard(data);
                break;
            case SelectionCardOutcome.TAKE:
                data.owner.loseCard(data);
                data.acquire(PlayerAgent.instance);
                PlayerAgent.instance.acquireCard(data);
                break;
            case SelectionCardOutcome.TAKE_COPY:
                if(data.cardType == CardType.ACTION)
                {
                    ActionCard copy = new ActionCard(data as ActionCard);
                    data.acquire(PlayerAgent.instance);
                    PlayerAgent.instance.acquireCard(copy);
                }
                else if(data.cardType == CardType.GOLD)
                {
                    GoldCard copy = new GoldCard(data as GoldCard);
                    data.acquire(PlayerAgent.instance);
                    PlayerAgent.instance.acquireCard(copy);
                }
                break;
        }

        allowedSelections--;

        if(allowedSelections <= 0)
        {
            yield return new WaitForSeconds(waitSec);

            foreach (SelectionCard sc in cardsInSelectionWindow)
            {
                Destroy(sc.transform.parent.gameObject);
            }

            cardsInSelectionWindow.Clear();

            container.SetActive(false);
            Total_UI.instance.changeUIState(Current_UI_State.PlayerTurn);
        } else
        {
            Total_UI.instance.changeUIState(Current_UI_State.SelectionWindow);
        }
    }

    public void displayReaction(PersonCard withCard, Agent agent)
    {
        Debug.Log("Display reaction selection...");
        Total_UI.instance.changeUIState(Current_UI_State.ReactionWindow);
        container.SetActive(true);

        actionCardAtStake = withCard;

        List<Card> cardsToGet = new List<Card>();

        int count = 0;
        foreach (Card card in cardsToGet)
        {
            if(isReactionCard(card))
            {
                GameObject template;
                switch (card.cardType)
                {
                    case CardType.ACTION: template = selectionCardTemplate_action; break;
                    default: template = selectionCardTemplate_gold; break;
                }
                GameObject go = GameObject.Instantiate(template, this.transform);

                SelectionCard sCard = go.GetComponentInChildren<SelectionCard>();
                sCard.initialize(card, true);

                go.transform.localPosition = defaultPos + new Vector3(3 * (count % maxInRow), -5 * (count / maxInRow), 0);

                cardsInSelectionWindow.Add(sCard);

                count++;
            }
        }
    }

    public void madeChoiceReaction(SelectionCard chosen)
    {
        StartCoroutine(madeChoiceReactionCo(chosen));
    }

    private IEnumerator madeChoiceReactionCo(SelectionCard chosen)
    {
        int waitSec = 1;

        // Do something with the selection
        switch (instance.currentOutcome)
        {
            case SelectionCardOutcome.BLOCK:
                Debug.Log("Would block the action here");
                break;
            case SelectionCardOutcome.REVERSE:
                Debug.Log("Would reverse the action here");
                break;
            default:
                Debug.Log("Must allow action");
                break;

                // TODO assassination = lose a card
        }

        yield return new WaitForSeconds(waitSec);

        foreach (SelectionCard sc in cardsInSelectionWindow)
        {
            Destroy(sc.transform.parent.gameObject);
        }

        cardsInSelectionWindow.Clear();

        container.SetActive(false);
        Total_UI.instance.changeUIState(Current_UI_State.PlayerTurn);
    }

    private bool isReactionCard(Card c)
    {
        if(c is ActionCard)
        {
            return (c as ActionCard).actionCardType == ActionCardType.BODYGUARD;
        } 
        else if(c is GoldCard)
        {
            return (c as GoldCard).goldCardType == GoldCardType.DOUBLE_AGENT;
        }
        return false;
    }

    public enum SelectionCardOutcome
    {
        // Complete actions
        REDACTION,
        VIEW,
        DECLASS,
        TAKE,
        TAKE_COPY,
        // Reactions
        ALLOW,
        BLOCK,
        REVERSE,
        LOSE
    }
}
