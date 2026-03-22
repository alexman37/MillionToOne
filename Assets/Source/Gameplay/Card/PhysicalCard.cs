using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Controls all physical aspects of the card (clicking, hovering, etc.)
/// </summary>
public class PhysicalCard : ConditionalUI, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 normalPosition;
    private Vector3 raisedPosition;

    private Coroutine activeCo;
    private bool markedForRemoval = false;

    protected Card data;

    private void Start()
    {
        allowedGameStates = new HashSet<Current_UI_State>() { Current_UI_State.PlayerTurn };

        if (allowedGameStates.Contains(Total_UI.uiState)) activeUI = true;
    }

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
        if(activeUI && !markedForRemoval)
        {
            if(activeCo != null)
            {
                StopCoroutine(activeCo);
            }
            activeCo = StartCoroutine(moveToUpPosition());
        }
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (activeUI && !markedForRemoval)
        {
            if (activeCo != null)
            {
                StopCoroutine(activeCo);
            }
            activeCo = StartCoroutine(returnToDownPosition());
        }
    }

    private IEnumerator moveToUpPosition()
    {
        float timeTake = 0.25f;
        Vector3 start = transform.localPosition;
        Vector3 end = raisedPosition;

        for (float i = 0; i < timeTake; i += Time.deltaTime)
        {
            transform.localPosition = UITransitions.smoothStep(start, end, i / timeTake);
            yield return null;
        }

        transform.localPosition = end;
    }

    private IEnumerator returnToDownPosition()
    {
        float timeTake = 0.25f;
        Vector3 start = transform.localPosition;
        Vector3 end = normalPosition;

        for (float i = 0; i < timeTake; i += Time.deltaTime)
        {
            transform.localPosition = UITransitions.smoothStep(start, end, i / timeTake);
            yield return null;
        }

        transform.localPosition = end;
    }

    private IEnumerator throwDown()
    {
        float timeTake = 0.5f;
        Vector3 start = transform.localPosition;
        Vector3 end = new Vector3(Screen.width / 2 + Random.Range(-60, 60), Screen.height / 2 + Random.Range(-60, 60), -60);
        Quaternion endRotation = Quaternion.Euler(0, 0, Random.Range(-37f, 37f));

        for (float i = 0; i < timeTake; i += Time.deltaTime)
        {
            transform.localPosition = UITransitions.smoothStep(start, end, i / timeTake);
            transform.localRotation = Quaternion.Lerp(Quaternion.identity, endRotation, i / timeTake);
            yield return null;
        }

        transform.localPosition = end;
        yield return new WaitForSeconds(0.75f);

        PlayerAgent.instance.playCard(data);
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (activeUI)
        {
            markedForRemoval = true;

            Debug.Log("Attempted to play card");
            if (activeCo != null)
            {
                StopCoroutine(activeCo);
            }
            activeCo = StartCoroutine(throwDown());
        }
    }
}
