using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace MapMagic
{
    [System.Serializable]
    [GeneratorMenu(menu = "CustomObjects", name = "Cliff Floor", disengageable = true)]
    public class StackGenerator : Generator
    {
        public Input objsIn = new Input("Input", InoutType.Objects, write: false, mandatory: true);
        public Output objsOut = new Output("Output", InoutType.Objects);

        public float Probability = 0.5f;
        public float SizeStackMultiplier = 1;
        public float HeightGain = 0.05f;
        public float SizeFactor = 0.5f;

        public override IEnumerable<Input> Inputs()
        {
            yield return objsIn;
        }

        public override IEnumerable<Output> Outputs()
        {
            yield return objsOut;
        }

        public override void Generate(Chunk chunk, Biome generatingBiome = null)
        {
            //getting inputs
            SpatialHash objs = (SpatialHash)objsIn.GetObject(chunk);
            SpatialHash outObjs = chunk.defaultSpatialHash;

            //return on stop/disable/null input
            if (chunk.stop)
            {
                return;
            }
            if (!enabled || objs == null)
            {
                objsOut.SetObject(chunk, outObjs);
                return;
            }

            var random = new System.Random(MapMagic.instance.seed + seed + chunk.coord.GetHashCode());

            foreach (var obj in objs.AllObjs())
            {
                var roll = (float)random.NextDouble();
                if (roll > Probability)
                {
                    continue;
                }
                outObjs.Add(
                    obj.pos, 
                    obj.height + HeightGain * Mathf.Lerp(1, obj.sizeScalar, SizeFactor), 
                    Quaternion.identity, 
                    obj.size * SizeStackMultiplier,
                    new List<StringTuple>());
            }

            //setting output
            if (chunk.stop) return;
            objsOut.SetObject(chunk, outObjs);
        }

        public override void OnGUI()
        {
            //inouts
            layout.Par(20);
            objsIn.DrawIcon(layout);
            objsOut.DrawIcon(layout);

            layout.Par(5);

            //params
            layout.Field(ref seed, "Seed");
            layout.Field(ref Probability, "Probability");
            layout.Field(ref SizeStackMultiplier, "Stack Size");
            layout.Field(ref HeightGain, "Height Gain");
            layout.Field(ref SizeFactor, "Size Factor");
        }
    }
}