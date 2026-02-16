using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterCard : MonoBehaviour
{
    public int characterId;

    public static event Action<int, bool> charCardClicked = (_,__) => { };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnClick()
    {
        Debug.Log("Correct ID= " + TurnDriver.instance.currentRoster.targetId + " YourID= " + characterId);
        bool correct = TurnDriver.instance.currentRoster.targetId == characterId;
        charCardClicked.Invoke(characterId, correct);
    }
}
