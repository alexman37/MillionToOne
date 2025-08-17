using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class CPD_HairColor : CPD<HairColor>
{
    public static CPD_HairColor instance;
    public CPD_HairColor() : base("hairTonesTxt", null, CPD_Type.Color)
    {
        if (instance == null)
        {
            instance = this;
        }
    }
}

//define the characteristics of this property here
public class HairColor : CPD_Field_Color
{
    // Need the empty constructor so that we can use generic typing in the CPD file
    public HairColor() : base(-1, "", new string[0], 0, Color.white)
    {
    }
    public HairColor(int id, Color32 color, string name, float probability, string[] general) : base(id, name, general, probability, color)
    {
    }
}