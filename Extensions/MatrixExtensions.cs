using System;
using UnityEngine;

namespace MapMagic
{
    public static class MatrixExtensions
    {
        public static float GetTotalAverage(this Matrix m)
        {
            var total = 0f;
            for (var i = 0; i < m.array.Length; i++)
            {
                total += m.array[i];
            }
            return total/m.array.Length;
        }

        public static void DrawCircle(this Matrix matrix, Coord origin, int size = 1, float value = 1,
            BlendGenerator.Algorithm algorithm = BlendGenerator.Algorithm.add)
        {
            for (var i = -size; i <= size; ++i)
            {
                for (var j = -size; j <= size; ++j)
                {
                    var dist = Vector2.Distance(Vector2.zero, new Vector2(i, j))/size;
                    if (dist < 1)
                    {
                        matrix.SafeSet(origin.x + i, origin.z + j, value, algorithm);
                    }
                }
            }
        }

        

        public delegate bool PlotFunction(int x, int z, int width, Coord start, Coord end);

        public static void DrawLine(this Matrix matrix, Coord start, Coord end, int width, PlotFunction plot)
        {
            var x0 = start.x;
            var z0 = start.z;

            var x1 = end.x;
            var z1 = end.z;
            
            bool steep = Math.Abs(z1 - z0) > Math.Abs(x1 - x0);
            if (steep)
            {
                ListExtensions.Swap(ref x0, ref z0);
                ListExtensions.Swap(ref x1, ref z1);
            }
            if (x0 > x1)
            {
                ListExtensions.Swap(ref x0, ref x1);
                ListExtensions.Swap(ref z0, ref z1);
            }

            int dX = (x1 - x0);
            int dY = Math.Abs(z1 - z0);
            int err = (dX / 2);
            int ystep = (z0 < z1 ? 1 : -1);
            int z = z0;

            for (int x = x0; x <= x1; ++x)
            {
                if (!(steep ? plot(z, x, width, start, end) : plot(x, z, width, start, end)))
                {
                    return;
                }
                err = err - dY;
                if (err < 0) { z += ystep; err += dX; }
            }
        }

        public static void SafeSet(this Matrix matrix, int x, int z, float val,
            BlendGenerator.Algorithm algorithm = BlendGenerator.Algorithm.max)
        {
            if (x >= matrix.rect.Min.x && x < matrix.rect.Max.x && z >= matrix.rect.Min.z && z < matrix.rect.Max.z)
            {
                var alg = BlendGenerator.GetAlgorithm(algorithm);
                matrix[x, z] = alg(matrix[x, z], val);
            }
        }
    }
}