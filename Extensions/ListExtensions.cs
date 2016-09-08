using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class ListExtensions
{
    public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
    {
        var aux = list[newIndex];
        list[newIndex] = list[oldIndex];
        list[oldIndex] = aux;
    }

    public static T Random<T>(this IList<T> array)
    {
        if (array.Count == 0)
        {
            throw new Exception("Check for empty arrays before calling this!");
        }
        if (array.Count == 1)
        {
            return array[0];
        }
        return array[UnityEngine.Random.Range(0, array.Count)];
    }

    public static void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }
}