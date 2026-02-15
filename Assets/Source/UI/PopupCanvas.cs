using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupCanvas : MonoBehaviour
{
    public static PopupCanvas instance;

    public Canvas popupCanvas;
    public GameObject targetPropertyGuess;
    private List<TargetCharGuess> targetPropertyEntries = new List<TargetCharGuess>();

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(this);
    }

    // Player guesses a characteristic of the target
    public void popup_targetPropertyGuess(CPD_Type cpdType)
    {
        GameObject targetPropertyEntry = targetPropertyGuess.transform.GetChild(0).gameObject;
        List<string> cats = Roster.cpdByType[cpdType].categories;

        for (int i = 0; i < cats.Count; i++)
        {
            string cat = cats[i];

            // Instantiate buttons for guessing
            GameObject next = GameObject.Instantiate(targetPropertyEntry, targetPropertyGuess.transform);
            next.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, -60 * i);

            TargetCharGuess tcg = next.GetComponent<TargetCharGuess>();
            tcg.initialize(cpdType, cat);
            targetPropertyEntries.Add(tcg);
            next.SetActive(true);

            popupCanvas.enabled = true;
            targetPropertyGuess.SetActive(true);
        }
    }

    public void popup_targetPropertyClear()
    {
        foreach (TargetCharGuess go in targetPropertyEntries)
        {
            Destroy(go.gameObject);
        }
        targetPropertyEntries.Clear();

        popupCanvas.enabled = false;
        targetPropertyGuess.SetActive(false);
    }
}
