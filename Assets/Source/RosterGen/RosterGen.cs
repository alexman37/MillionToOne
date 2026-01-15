using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Handles creation of a Roster.
/// TODO maybe more to do with "remaking" the roster on subsequent plays.
/// </summary>
public class RosterGen : MonoBehaviour
{
    public int numberOfCharacters;
    Roster roster;

    public static event Action<Roster> rosterCreationDone;

    // Start is called before the first frame update
    void Start()
    {
        //TODO i don't believe size should be passed in anymore. it should be determined only by constrainable CPDs
        roster = new Roster();
        //roster.DebugLogRoster();
        rosterCreationDone += (_) => { };

        rosterCreationDone.Invoke(roster);
    }
}
