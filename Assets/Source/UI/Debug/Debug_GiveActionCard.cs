using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_GiveActionCard : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void givePlayerActionCard()
    {
        TurnDriver.instance.dealActionCard();
    }
}
