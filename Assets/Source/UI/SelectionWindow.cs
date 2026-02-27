using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Appears when the player uses an Action card that requires selecting a card from someone's hand.
/// </summary>
public class SelectionWindow : ConditionalUI
{
    public static SelectionWindow instance;

    private static PlayerAgent playerAgent;

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


    public static event Action<Agent, Agent, ReactionVerdict> playerReacts = (a1,a2,v) => { };


    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        allowedGameStates = new HashSet<Current_UI_State>() { Current_UI_State.SelectionWindow, Current_UI_State.ReactionWindow };
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }


    public void prepareForDisplay(SelectionCardOutcome outcome, int times, int whatKinds, bool cardsFaceUp)
    {
        currentOutcome = outcome;
        allowedSelections = times;

        as_whatKinds = whatKinds;
        as_cardsFaceUp = cardsFaceUp;
    }

    /// <summary>
    /// whatKinds: 0 = clue cards only, 1 = action cards only, 2 = all
    /// </summary>
    public void displaySelection(SelectionCardOutcome outcome, int times, Agent agent, int whatKinds, bool cardsFaceUp)
    {
        if (playerAgent == null) playerAgent = PlayerAgent.instance;

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
                playerAgent.playCard(data);
                break;
            case SelectionCardOutcome.VIEW:
                chosen.Reveal();
                if(data.cardType == CardType.CLUE)
                {
                    playerAgent.onClueCardDeclassified(data as ClueCard);
                }
                waitSec = 2;
                break;
            case SelectionCardOutcome.DECLASS:
                // TODO no reward for this
                (data as ClueCard).owner.playCard(data);
                break;
            case SelectionCardOutcome.TAKE:
                data.owner.loseCard(data);
                data.acquire(playerAgent);
                playerAgent.acquireCard(data);
                break;
            case SelectionCardOutcome.TAKE_COPY:
                if(data.cardType == CardType.ACTION)
                {
                    ActionCard copy = new ActionCard(data as ActionCard);
                    data.acquire(playerAgent);
                    playerAgent.acquireCard(copy);
                }
                else if(data.cardType == CardType.GOLD)
                {
                    GoldCard copy = new GoldCard(data as GoldCard);
                    data.acquire(playerAgent);
                    playerAgent.acquireCard(copy);
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
        if (playerAgent == null) playerAgent = PlayerAgent.instance;

        Debug.Log("Display reaction selection...");
        Total_UI.instance.changeUIState(Current_UI_State.ReactionWindow);
        container.SetActive(true);

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

    public void madeNoReaction()
    {
        StartCoroutine(madeChoiceReactionCo(null));
    }

    private IEnumerator madeChoiceReactionCo(SelectionCard chosen)
    {
        Agent playingAgent = TurnDriver.instance.queuedCard.owner;
        int waitSec = 1;

        // TODO dispatch the action to TurnDriver
        if (chosen == null)                               playerReacts.Invoke(playingAgent, playerAgent, ReactionVerdict.ALLOW);
        else if (chosen.data.cardType == CardType.ACTION) playerReacts.Invoke(playingAgent, playerAgent, ReactionVerdict.BLOCK);
        else if (chosen.data.cardType == CardType.GOLD)   playerReacts.Invoke(playingAgent, playerAgent, ReactionVerdict.REVERSE);

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
        LOSE          // Lose a card after assassination
    }
}

public enum ReactionVerdict
{
    ALLOW,
    BLOCK,
    REVERSE,
}
