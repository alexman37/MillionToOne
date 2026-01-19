using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    /// <summary>
    /// Shuffle a list of type T in place, return the new shuffled list
    /// </summary>
    public static List<T> Shuffle<T>(List<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
        return ts;
    }


    /// <summary>
    /// Return the abbreviated string representation of a number   \  
    /// Ex: 1,567,000  ->  1.5M    \  
    /// No more than 4 characters allowed   \  
    /// Assume number is positive and less than 999T
    /// </summary>
    public static (string, NumberGrouping) AbbreviatedNumber(int number)
    {
        int numDigits = Mathf.CeilToInt(Mathf.Log10(number) - 1);
        
        switch(numDigits)
        {
            case 0:
                return (number.ToString(), NumberGrouping.Ones);
            case 1:
                return (number.ToString(), NumberGrouping.Tens);
            case 2:
                return (number.ToString(), NumberGrouping.Hundreds);
            case 3:
                return (decimalify(number) + "K", NumberGrouping.Thousands);
            case 4:
                return (chop(number, 2) + "K", NumberGrouping.MoreThanThousands);
            case 5:
                return (chop(number, 3) + "K", NumberGrouping.MoreThanThousands);
            case 6:
                return (decimalify(number) + "M", NumberGrouping.MoreThanThousands);
            case 7:
                return (chop(number, 2) + "M", NumberGrouping.MoreThanThousands);
            case 8:
                return (chop(number, 3) + "M", NumberGrouping.MoreThanThousands);
            case 9:
                return (decimalify(number) + "B", NumberGrouping.MoreThanThousands);
            case 10:
                return (chop(number, 2) + "B", NumberGrouping.MoreThanThousands);
            case 11:
                return (chop(number, 3) + "B", NumberGrouping.MoreThanThousands);
            case 12:
                return (decimalify(number) + "T", NumberGrouping.MoreThanThousands);
            case 13:
                return (chop(number, 2) + "T", NumberGrouping.MoreThanThousands);
            case 14:
                return (chop(number, 3) + "T", NumberGrouping.MoreThanThousands);
            default:
                return ("???", NumberGrouping.MoreThanThousands);
        }
    }

    // Assume number is greater than 9
    private static string decimalify(int number)
    {
        float inter = (float)number / Mathf.Pow(10, Mathf.Ceil(Mathf.Log10(number) - 1));
        return inter.ToString("0.0");
    }

    // Assume number is greater than 9
    private static string chop(int number, int numDigits)
    {
        float inter = (float)number / Mathf.Pow(10, Mathf.Ceil(Mathf.Log10(number) - 1));
        int f = Mathf.RoundToInt(inter * Mathf.Pow(10, numDigits - 1));
        return f.ToString();
    }
}


public enum NumberGrouping
{
    Ones,
    Tens,
    Hundreds,
    Thousands,
    MoreThanThousands
}