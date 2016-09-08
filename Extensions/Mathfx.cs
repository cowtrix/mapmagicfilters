using System;

public class Mathfx
{  
    public static int Clamp(int val, int min, int max)
    {
        val = Math.Max(min, val);
        val = Math.Min(max, val);
        return val;
    }
}