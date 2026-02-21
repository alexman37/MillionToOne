using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PhysicalClueCard : PhysicalCard
{
    [SerializeField] private TextMeshProUGUI cpdNum;
    [SerializeField] private TextMeshProUGUI cpdTitle;
    [SerializeField] private TextMeshProUGUI categoryText;

    [SerializeField] private Image[] redactions;

    public override void initialize()
    {
        base.initialize();

        ClueCard cc = data as ClueCard;
        cpdNum.text = ((int)cc.cpdType + 1).ToString();
        cpdTitle.text = cc.cpdType.ToString();
        categoryText.text = cc.category;
    }

    public void redact()
    {
        foreach(Image img in redactions)
        {
            img.enabled = true;
        }
    }
}
