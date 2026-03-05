using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


// A card that's purely cosmetic
// Looks like a normal card, but can't be interacted with
public class DisplayCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cpdCode;
    [SerializeField] private TextMeshProUGUI cpdTitle;
    [SerializeField] private TextMeshProUGUI category;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void initWith(string cpdCode, string cpdTitle, string cat)
    {
        this.cpdCode.text = cpdCode;
        this.cpdTitle.text = cpdTitle;
        this.category.text = cat;
    }
}
