using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Controls all physical aspects of the card (clicking, hovering, etc.)
/// </summary>
public class PhysicalCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 normalPosition;
    private Vector3 raisedPosition;

    protected Card data;

    // You must ensure the card component is created first, so we can't throw this in start
    public virtual void initialize()
    {
        normalPosition = transform.localPosition;
        raisedPosition = new Vector3(normalPosition.x, normalPosition.y + transform.localScale.y / 2, -50);
    }

    public void setData(Card d)
    {
        data = d;
    }

    public Card getData()
    {
        return data;
    }

    // When you remove a card behind this one in the order, it must get moved back a constant amount
    public void bumpBackOne()
    {
        Vector3 interval = new Vector3(50, 0, 0);
        transform.localPosition -= interval;
        normalPosition -= interval;
        raisedPosition -= interval;
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        transform.localPosition = raisedPosition;
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        transform.localPosition = normalPosition;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        Debug.Log("Attempted to play card");
        PlayerAgent.instance.playCard(data);
    }
}
