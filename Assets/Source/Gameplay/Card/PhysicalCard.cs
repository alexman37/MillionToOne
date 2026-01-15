using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Controls all physical aspects of the card (clicking, hovering, etc.)
/// </summary>
public class PhysicalCard : MonoBehaviour
{
    // TODO replace with a picture eventually
    [SerializeField] private TextMeshProUGUI text;
    private RectTransform rt;

    private Card data;

    private Total_UI totalUI;

    // You must ensure the card component is created first, so we can't throw this in start
    public void initialize()
    {
        text.text = data.ToString();

        totalUI = FindObjectOfType<Total_UI>();
    }

    public void setData(Card d)
    {
        data = d;
    }

    public Card getData()
    {
        return data;
    }

    private void OnMouseDown()
    {
        Debug.Log("You clicked the card ");
    }

    private void OnMouseEnter()
    {
        Debug.Log("You started hovering over this card");
    }
}
