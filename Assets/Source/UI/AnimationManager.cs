using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager instance;

    private Playerbase playerbase;

    public static event Action<int> cpuReadyForProcess;

    private Coroutine activeCo;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        playerbase = FindObjectOfType<Playerbase>();
    }

    public void BeginTurnForPlayer()
    {
        playerbase.moveTurnMarker(0);
    }

    public void BeginTurnForCPU(CPUAgent who)
    {
        if(activeCo == null)
        {
            activeCo = StartCoroutine(BeginTurnForCPUco(who));
        }
    }

    private IEnumerator BeginTurnForCPUco(CPUAgent who)
    {
        playerbase.moveTurnMarker(who.id);
        // Wait 1 second before the CPU starts doing stuff
        yield return new WaitForSeconds(1);

        who.agentLogic.processTurn();
        activeCo = null;
    }
}

public static class UITransitions
{
    public static Vector3 smoothStep(Vector3 start, Vector3 target, float t)
    {
        float inter = 3 * Mathf.Pow(t, 2) - 2 * Mathf.Pow(t, 3);
        return (1 - inter) * start + inter * target;
    }

    public static Vector3 Xerp(Vector3 start, Vector3 target, float t)
    {
        float inter = Mathf.Sqrt(t);
        return (1 - inter) * start + inter * target;
    }
}
