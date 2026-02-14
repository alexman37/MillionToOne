using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

/// <summary>
/// A Clue card contains information about what the target is NOT.
/// So, for each demographic, the target will have the characteristic that no players have
/// You generally hold onto these until you decide to declassify them (revealing them to everyone else) or are forced to give them up.
/// Holding a clue card immediately and permanently marks that characteristic off on the roster form.
/// </summary>
public class ClueCard : Card
{
    public CPD_Type cpdType;        // What CPD is this card about?
    public string category;         // What category is it identifying?
    public bool onTarget = false;   // If true, the target has this category

    private bool redacted = false;   // If true, no one can see what was on this card, making it functionally useless

    public static event Action<ClueCard> clueCardDeclassified;


    public ClueCard(CPD_Type cpdType, string cat, bool onTarget)
    {
        cardType = CardType.CLUE;

        this.cpdType = cpdType;
        this.category = cat;
        this.onTarget = onTarget;

        //GetComponentInChildren<TextMeshProUGUI>().text = cpdType + ": " + cat;
    }

    public override void acquire()
    {

    }

    public override void play()
    {
        // Update the common constraints. Everyone knows this information now.
        clueCardDeclassified.Invoke(this);
    }

    // One redacted, there's nothing you can do to get it back.
    public void redact()
    {
        redacted = true;
    }

    public override string ToString()
    {
        return "Clue card for " + cpdType + ": " + category;
    }
}
