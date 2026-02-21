using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PhysicalClueCard : PhysicalCard
{
    [SerializeField] private TextMeshProUGUI cpdNum;
    [SerializeField] private TextMeshProUGUI cpdTitle;
    [SerializeField] private TextMeshProUGUI categoryText;


    public override void initialize()
    {
        base.initialize();

        ClueCard cc = data as ClueCard;
        cpdNum.text = ((int)cc.cpdType + 1).ToString();
        cpdTitle.text = cc.cpdType.ToString();
        categoryText.text = cc.category;
    }
}
