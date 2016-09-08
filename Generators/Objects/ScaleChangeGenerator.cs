using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapMagic
{
    [Serializable]
    [GeneratorMenu(menu = "CustomObjects", name = "Scale", disengageable = true)]
    public class ScaleChangeGenerator : Generator
    {
        public Input input = new Input("Input", InoutType.Objects, false, true);
        public Output output = new Output("Output", InoutType.Objects);

        public bool Absolute;
        public float scaler = 1;
        public float scalerMax = 1;
        public float sizeFactor;

        public override IEnumerable<Input> Inputs()
        {
            yield return input;
        }

        public override IEnumerable<Output> Outputs()
        {
            yield return output;
        }

        public override void Generate(Chunk chunk, Biome generatingBiome = null)
        {
            //getting inputs
            var sourceHash = (SpatialHash)input.GetObject(chunk);
            if (sourceHash == null) return;
            var spatialHash = sourceHash.Copy();

            //return on stop/disable/null input
            if (chunk.stop || spatialHash == null) return;
            if (!enabled)
            {
                output.SetObject(chunk, spatialHash);
                return;
            }

            var random = new System.Random(MapMagic.instance.seed + seed + chunk.coord.GetHashCode());
            
            //preparing output
            spatialHash = spatialHash.Copy();

            foreach (var obj in spatialHash.AllObjs())
            {
                var scalerVal = scaler + (float) random.NextDouble()*(scalerMax - scaler);
                if (Absolute)
                {
                    obj.size = Vector3.one * scalerVal;
                }
                else
                {
                    float percent = 1;
                    //everything else does
                    percent = percent * (1 - sizeFactor) + percent * obj.sizeScalar * sizeFactor;
                    obj.size *= scalerVal * percent;
                }
                
            }

            //setting output
            if (chunk.stop) return;
            output.SetObject(chunk, spatialHash);
        }

        public override void OnGUI()
        {
            layout.margin = 18;
            layout.rightMargin = 15;

            //inouts
            input.DrawIcon(layout); 
            output.DrawIcon(layout);

            if (scalerMax < scaler)
            {
                scalerMax = scaler;
            }

            //			layout.inputSize = 0.5f;
            layout.Field(ref scaler, "Min");
            layout.Field(ref scalerMax, "Max");
            layout.Field(ref sizeFactor, "Size Factor");
            layout.Field(ref Absolute, "Absolute");
        }
    }
}