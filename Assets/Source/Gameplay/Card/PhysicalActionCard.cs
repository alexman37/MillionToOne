using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PhysicalActionCard : PhysicalCard
{
    [SerializeField] private TextMeshProUGUI personTitle;
    [SerializeField] private Image pic;

    public override void initialize()
    {
        base.initialize();

        PersonCard pc = data as PersonCard;
        personTitle.text = pc.ToString();
    }

    public void reinit(PersonCard newCard)
    {
        setData(newCard);
        PersonCard pc = data as PersonCard;
        personTitle.text = pc.ToString();
    }
}
