using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Determines how CPUs weight playing action cards
/// Does NOT control what happens when one is played - see AcitonHandler_CPU
/// </summary>
public class CPU_AL_ActionLogic
{
    private CPUAgent selfAgent;
    private CPUPersonalityStats personality;

    public CPU_AL_ActionLogic(CPUAgent self)
    {
        selfAgent = self;
        personality = self.personalityStats;
    }


    // TODO pick the person with most threat, maybe?
    public Agent getBestTarget(PersonCard pc)
    {
        return PlayerAgent.instance;
    }

    // TODO depends on personality
    public Card getBestCardToRedact(Agent whoseHand)
    {
        return whoseHand.inventory[0];
    }

    // TODO depends on personality
    public Card getBestCardToCopy(Agent whoseHand)
    {
        return whoseHand.recruits[0];
    }

    // TODO some personalities like action cards more, others like clue cards
    public Card pickSemirandomCard(Agent whoseHand, int types)
    {
        return whoseHand.inventory[0];
    }

    public List<Card> pickSemirandomCards(int howMany, Agent whoseHand, int types)
    {
        List<Card> cards = new List<Card>();
        cards.AddRange(whoseHand.inventory.GetRange(0, howMany));
        return cards;
    }

    // TODO
    public int getActionCardScore(PersonCard pc)
    {
        return 1000;
    }
}
