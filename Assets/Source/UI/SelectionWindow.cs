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
    [SerializeField] GameObject selectionCardTemplate;

    List<SelectionCard> cardsInSelectionWindow = new List<SelectionCard>();

    Vector3 defaultPos = new Vector3(-10, 21, 0);
    int maxInRow = 5;
    int allowedSelections = 1;
    public SelectionCardOutcome currentOutcome;


    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    /// <summary>
    /// whatKinds: 0 = clue cards only, 1 = action cards only, 2 = all
    /// </summary>
    public void displaySelection(SelectionCardOutcome outcome, int times, Agent agent, int whatKinds, bool cardsFaceUp)
    {
        Total_UI.instance.changeUIState(Current_UI_State.SelectionWindow);
        container.SetActive(true);

        currentOutcome = outcome;

        List<Card> cardsToGet = new List<Card>();
        if (whatKinds != 1) cardsToGet.AddRange(agent.inventory);
        if (whatKinds != 0) cardsToGet.AddRange(agent.recruits);

        int count = 0;
        foreach (Card card in cardsToGet)
        {
            GameObject go = GameObject.Instantiate(selectionCardTemplate, this.transform);
            SelectionCard sCard = go.GetComponentInChildren<SelectionCard>();
            sCard.initialize(card, cardsFaceUp);

            go.transform.localPosition = defaultPos + new Vector3(3 * (count % maxInRow), -5 * (count / maxInRow), 0);

            cardsInSelectionWindow.Add(sCard);

            count++;
        }
    }

    public void madeChoice(SelectionCard chosen)
    {
        Card data = chosen.data;

        // Do something with the selection
        switch (instance.currentOutcome)
        {
            case SelectionCardOutcome.REDACTION:
                (data as ClueCard).redact();
                PlayerAgent.instance.playCard(data);
                break;
            case SelectionCardOutcome.VIEW:
                chosen.Reveal();
                break;
            case SelectionCardOutcome.DECLASS:
                // TODO no reward for this
                (data as ClueCard).owner.playCard(data);
                break;
            case SelectionCardOutcome.TAKE:
                data.acquire(PlayerAgent.instance);
                // TODO ensure transfer
                break;
            case SelectionCardOutcome.TAKE_COPY:
                data.acquire(PlayerAgent.instance);
                // TODO ensure transfer
                break;
        }


        foreach (SelectionCard sc in cardsInSelectionWindow)
        {
            Destroy(sc.transform.parent.gameObject);
        }

        cardsInSelectionWindow.Clear();

        container.SetActive(false);
        Total_UI.instance.changeUIState(Current_UI_State.PlayerTurn);
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
