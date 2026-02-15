using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetProperties : MonoBehaviour
{
    [SerializeField] private GameObject targetCardTemplate;

    public List<TargetCard> targetCards;

    public Vector3 defaultTargetPosition;

    private Character targetCharacter;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        RosterGen.rosterCreationDone += prepareTargetCards;
    }

    private void OnDisable()
    {
        RosterGen.rosterCreationDone -= prepareTargetCards;
    }

    // Given a character who will serve as the target, prepare all target cards for use in game.
    void prepareTargetCards(Roster rost)
    {
        targetCharacter = rost.getTargetAsCharacter();

        for(int i = 0; i < Roster.cpdConstrainables.Count; i++)
        {
            CPD cpd = Roster.cpdConstrainables[i];

            GameObject next = GameObject.Instantiate(targetCardTemplate, transform);

            next.transform.localPosition = defaultTargetPosition + new Vector3(2 * i, 0, 0);
            TargetCard tc = next.GetComponentInChildren<TargetCard>();
            tc.initialize(i, cpd.cpdType.ToString(), targetCharacter.getCategoryofCharacteristic(cpd.cpdType));

            targetCards.Add(tc);
        }
        
    }
}
