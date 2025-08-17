using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class CPD_SkinTone : CPD<SkinTone>
{
    public static CPD_SkinTone instance;
    public CPD_SkinTone() : base("skinTonesTxt", null, CPD_Type.Color)
    {
        if (instance == null)
        {
            instance = this;
        }
    }
}

//define the characteristics of this property here
public class SkinTone : CPD_Field_Color
{
    // Need the empty constructor so that we can use generic typing in the CPD file
    public SkinTone() : base(-1, "", new string[0], 0, Color.white)
    {
    }
    public SkinTone(int id, Color32 color, string name, float probability, string[] general) : base(id, name, general, probability, color)
    {
    }
}