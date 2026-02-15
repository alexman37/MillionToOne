using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TargetCard : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI cpdNum;
    [SerializeField] private TextMeshProUGUI cpdTitle;
    [SerializeField] private TextMeshProUGUI category;
    [SerializeField] private Sprite pic;

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

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        Debug.Log("Attempted to guess target card");
    }
}
