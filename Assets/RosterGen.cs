using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RosterGen : MonoBehaviour
{
    public int numberOfCharacters;
    Roster roster;

    public static event Action<Roster> rosterCreationDone;

    // Start is called before the first frame update
    void Start()
    {
        roster = new Roster(100);
        roster.DebugLogRoster();
        rosterCreationDone.Invoke(roster);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
