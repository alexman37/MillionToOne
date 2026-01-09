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
        Debug.Log("Done gen i suppose");
        //TODO i don't believe size should be passed in anymore. it should be determined only by constrainable CPDs
        roster = new Roster();
        Debug.Log("Done gen i suppose 1");
        //roster.DebugLogRoster();
        rosterCreationDone += (_) => { };

        rosterCreationDone.Invoke(roster);
        Debug.Log("Done gen i suppose 2");
    }
}
