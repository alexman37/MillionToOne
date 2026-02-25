using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Appears when the player uses an Action card that requires selecting a card from someone's hand.
/// </summary>
public class SelectionWindow : MonoBehaviour
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


    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
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
            GameObject go = GameObject.Instantiate(card.cardType == CardType.CLUE ? 
                selectionCardTemplate_clue : selectionCardTemplate_action,
                this.transform);
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

    public enum SelectionCardOutcome
    {
        REDACTION,
        VIEW,
        DECLASS,
        TAKE,
        TAKE_COPY
    }
}
