using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPD_Hair : CPD<HairStyle>
{
    public static CPD_Hair instance;
    public CPD_Hair() : base("hairStylesTxt", "CharSprites/Hair/", CPD_Type.Filename)
    {
        if(instance == null)
        {
            instance = this;
        }
    }
}

//define the characteristics of this property here
public class HairStyle : CPD_Field_FileName
{
    // Need the empty constructor so that we can use generic typing in the CPD file
    public HairStyle() : base(-1, "", new string[0], 0, "")
    {
    }
    public HairStyle(int id, string filename, string name, float probability, string[] general) : base(id, name, general, probability, filename)
    {
    }
}