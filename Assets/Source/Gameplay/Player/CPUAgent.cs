using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CPUAgent : Agent
{
    int id;

    public static event Action<int, Card> cpuGotCard = (_,__) => { };
    public static event Action cpuTurnOver = () => { };

    public CPUAgent(int id, string name)
    {
        this.id = id;
        agentName = name;
    }

    public override void markAsReady()
    {
        Debug.Log("It's CPU player " + agentName + "'s turn.");
        Debug.LogWarning("Skipping turn as the CPU is not implemented yet.");
        cpuTurnOver.Invoke();
    }

    public override void acquireCard(Card card)
    {
        inventory.Add(card);
        Debug.Log("CPU player " + agentName + " acquires card: " + card);
    }

    public override void acquireCards(List<Card> cards)
    {
        inventory.AddRange(cards);
    }

    public override void playCard(Card card)
    {

    }

    public override void askAgent(Agent asking, List<(CPD_Type, string)> inquiry)
    {

    }

    public override void useAbility()
    {

    }
}
