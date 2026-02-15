using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class TargetCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cpdNum;
    [SerializeField] private TextMeshProUGUI cpdTitle;
    [SerializeField] private TextMeshProUGUI category;
    [SerializeField] private Sprite pic;

    [SerializeField] private Image selectedSpr;

    private bool revealed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void initialize(int num, string title, string cat)
    {
        cpdNum.text = num.ToString();
        cpdTitle.text = title;
        category.text = cat;
    }

    public void OnMouseEnter()
    {
        if(!revealed)
        {
            selectedSpr.gameObject.SetActive(true);
        }
    }

    public void OnMouseExit()
    {
        selectedSpr.gameObject.SetActive(false);
    }

    public void OnMouseDown()
    {
        Debug.Log("Attempted to guess target card");
    }
}
