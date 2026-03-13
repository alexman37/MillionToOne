using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoBar : MonoBehaviour
{
    public static InfoBar instance;

    public TextMeshProUGUI infoReadout;

    private Coroutine activeCo = null;

    // How long to type out the info bar text?
    float typeTime = 1f;

    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void setReadout(string toText)
    {
        if(activeCo != null)
        {
            StopCoroutine(activeCo);
        }
        activeCo = StartCoroutine(readoutTypeText(toText));
    }

    IEnumerator readoutTypeText(string toText)
    {
        float characterLen = toText.ToCharArray().Length;
        float timeForOneChar = typeTime / characterLen;

        float currTime = 0;
        int currPosition = 0;
        string currString = "";

        currString = toText[0].ToString();

        while(currTime < typeTime)
        {
            if(Time.deltaTime / timeForOneChar > 1)
            {
                int charsToAdd = Mathf.FloorToInt(Time.deltaTime / timeForOneChar);
                charsToAdd = Mathf.Min(charsToAdd, (int)characterLen - currPosition);
                currString = currString + toText.Substring(currPosition, charsToAdd);
                currPosition += charsToAdd;
            } else if(currTime + Time.deltaTime > (currPosition + 1) * timeForOneChar)
            {
                if(currPosition < characterLen - 1)
                {
                    currPosition += 1;
                    currString = currString + toText[currPosition];
                }
            }
            
            currTime += Time.deltaTime;
            infoReadout.text = currString;
            yield return null;
        }
        infoReadout.text = toText;
    }
}
