using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPD_Face : CPD<FaceType>
{
    public static CPD_Face instance;
    public CPD_Face() : base("faceTypesTxt", "CharSprites/Face/", CPD_Type.Filename)
    {
        if (instance == null)
        {
            instance = this;
        }
    }
}

//define the characteristics of this property here
public class FaceType : CPD_Field_FileName
{
    // Need the empty constructor so that we can use generic typing in the CPD file
    public FaceType() : base(-1, "", new string[0], 0, "")
    {
    }
    public FaceType(int id, string filename, string name, float probability, string[] general) : base(id, name, general, probability, filename)
    {
    }
}
