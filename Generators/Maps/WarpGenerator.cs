using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapMagic
{
    [Serializable]
    [GeneratorMenu(menu = "CustomMap", name = "Warp", disengageable = true)]
    public class WarpGenerator : Generator
    {
        public Input input = new Input("Input", InoutType.Map);
        public Input maskIn = new Input("Mask", InoutType.Map);
        public Input warpX = new Input("WarpX", InoutType.Map);
        public Input warpY = new Input("WarpY", InoutType.Map);

        public Output output = new Output("Output", InoutType.Map);

        public float borderSize = 0.05f;
        public float intensity = 1f;
        public bool preserveGradient = true;

        public override IEnumerable<Input> Inputs()
        {
            yield return input;
            yield return maskIn;
            yield return warpX;
            yield return warpY;
        }

        public override IEnumerable<Output> Outputs()
        {
            yield return output;
        }

        public override void Generate(Chunk chunk, Biome currentBiome = null)
        {
            borderSize = Mathf.Clamp(borderSize, float.Epsilon, 1);

            var refHeights = (Matrix)input.GetObject(chunk);
            if (refHeights != null)
            {
                refHeights = refHeights.Copy(null);
            }
            else
            {
                return;
            }

            var warpXMatrix = (Matrix) warpX.GetObject(chunk);
            var warpZMatrix = (Matrix) warpY.GetObject(chunk);

            if (chunk.stop) return;
            if (!enabled || intensity == 0 || (warpXMatrix == null && warpZMatrix == null))
            {
                output.SetObject(chunk, refHeights);
                return;
            }

            var matrix = chunk.defaultMatrix;

            var min = matrix.rect.Min;
            var max = matrix.rect.Max;
            var size = matrix.rect.size;
            for (var x = min.x; x < max.x; x++)
            {
                var distanceXMultiplier = (Math.Abs((size.x/2) - (x - min.x))/(size.x/2f));
                distanceXMultiplier = (distanceXMultiplier - (1 - borderSize))/borderSize;
                distanceXMultiplier = 1 - Mathf.Clamp01(distanceXMultiplier);

                for (var z = min.z; z < max.z; z++)
                {
                    var distanceZMultiplier = (Math.Abs((size.z/2) - (z - min.z))/(size.z/2f));
                    distanceZMultiplier = (distanceZMultiplier - (1 - borderSize))/borderSize;
                    distanceZMultiplier = 1 - Mathf.Clamp01(distanceZMultiplier);

                    var pos = new Vector2(x/(float) matrix.rect.size.x, z/(float) matrix.rect.size.z);
                    var warpZValue = 0f;
                    if (warpZMatrix != null)
                    {
                        var xPos = Mathf.FloorToInt(pos.x*warpZMatrix.rect.size.x);
                        var zPos = Mathf.FloorToInt(pos.y*warpZMatrix.rect.size.z);
                        xPos = Mathfx.Clamp(xPos, min.x, max.x);
                        zPos = Mathfx.Clamp(zPos, min.z, max.z);
                        try
                        {
                            warpZValue = warpZMatrix[xPos, zPos];
                            warpZValue *= 2;
                            warpZValue -= 1;
                        }
                        catch (Exception)
                        {
                            warpZValue = 0f;
                        }
                    }
                    var warpXValue = 0f;
                    if (warpXMatrix != null)
                    {
                        var xPos = Mathf.FloorToInt(pos.x*warpXMatrix.rect.size.x);
                        var zPos = Mathf.FloorToInt(pos.y*warpXMatrix.rect.size.z);
                        xPos = Mathfx.Clamp(xPos, min.x, max.x);
                        zPos = Mathfx.Clamp(zPos, min.z, max.z);
                        try
                        {
                            warpXValue = warpXMatrix[xPos, zPos];
                            warpXValue *= 2;
                            warpXValue -= 1;
                        }
                        catch (Exception)
                        {
                            warpXValue = 0f;
                        }
                    }

                    var scaledIntensity = (intensity/1024f)*MapMagic.instance.resolution;

                    warpXValue *= scaledIntensity * distanceXMultiplier * distanceZMultiplier;
                    warpZValue *= scaledIntensity * distanceXMultiplier * distanceZMultiplier;

                    if (preserveGradient)
                    {
                        try
                        {
                            float left = refHeights[Mathfx.Clamp(x - 1, min.x, max.x - 1), Mathfx.Clamp(z, min.z, max.z - 1)];
                            float up = refHeights[Mathfx.Clamp(x, min.x, max.x - 1), Mathfx.Clamp(z - 1, min.z, max.z - 1)];
                            float right = refHeights[Mathfx.Clamp(x + 1, min.x, max.x - 1), Mathfx.Clamp(z, min.z, max.z - 1)];
                            float down = refHeights[Mathfx.Clamp(x, min.x, max.x - 1), Mathfx.Clamp(z + 1, min.z, max.z - 1)];
                            var gradient = FindNormalFromPoints(left, up, right, down, MapMagic.instance.terrainHeight, MapMagic.instance.terrainSize);

                            // As it gets steeper, warp less
                            var steepness = gradient.y;
                            warpXValue *= steepness;
                            warpZValue *= steepness;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }

                    float warpedX = x + warpXValue;
                    //warpedX = Mathfx.Clamp(warpedX, min.x, max.x - 1);

                    float warpedZ = z + warpZValue;
                    //warpedZ = Mathfx.Clamp(warpedZ, min.z, max.z - 1);

                    try
                    {
                        matrix[x, z] = refHeights.GetInterpolatedValue(new Vector2(warpedX, warpedZ));
                    }
                    catch (Exception)
                    {
                        throw new Exception(string.Format("Tried to set {0},{1} to {2},{3}", x, z, warpedX, warpedZ));
                    }
                }
            }

            if (chunk.stop) return; //do not write object is generating is stopped
            output.SetObject(chunk, matrix);
        }

        public override void OnGUI()
        {
            //inouts
            layout.Par(20);
            input.DrawIcon(layout, "Input");
            output.DrawIcon(layout, "Output");
            layout.Par(20);
            warpX.DrawIcon(layout, "WarpX");
            layout.Par(20);
            warpY.DrawIcon(layout, "WarpY");
            layout.Par(20);
            maskIn.DrawIcon(layout, "Mask");
            layout.Par(6);

            //params
            layout.fieldSize = 0.5f;
            layout.Field(ref intensity, "Intensity");
            layout.Field(ref borderSize, "Border Size");
            layout.Field(ref preserveGradient, "Preserver Gradient");
        }

        public static Vector3 FindNormalFromPoints(float left, float up, float right, float down, float yScale, float xzScale)
        {
            var offsets = new float[4];
            offsets[0] = left;
            offsets[1] = up;
            offsets[2] = right;
            offsets[3] = down;

            xzScale = 1 / (yScale / xzScale);
            for (var i = 0; i < offsets.Length; i++)
            {
                offsets[i] *= yScale;
            }

            var va = (new Vector3(0, Mathf.Abs(offsets[0] - offsets[2]), xzScale)).normalized;
            var vb = (new Vector3(xzScale, Mathf.Abs(offsets[1] - offsets[3]), 0)).normalized;

            return (Vector3.Cross(va, vb)).normalized;
        }
    }
}