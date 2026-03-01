using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Debug_CPULogicPrintout : MonoBehaviour
{
    public static Debug_CPULogicPrintout instance;

    public TextMeshProUGUI[] printouts;
    public TextMeshProUGUI[] aaPrintouts;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void updatePrintout(int index, string text)
    {
        printouts[index].text = text;
    }

    public void updateAAprintout(int index, string text)
    {
        aaPrintouts[index].text = text;
    }
}
