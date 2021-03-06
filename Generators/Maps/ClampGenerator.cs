using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapMagic
{
    [Serializable]
    [GeneratorMenu(menu = "CustomMap", name = "Clamp", disengageable = true)]
    public class ClampGenerator : Generator
    {
        public Input input = new Input("Input", InoutType.Map, false); //, mandatory:true);
        public Input maskIn = new Input("Mask", InoutType.Map);
        public Output output = new Output("Output", InoutType.Map);

        public float Min = 0;
        public float Max = 1;

        public override IEnumerable<Input> Inputs()
        {
            yield return input;
            yield return maskIn;
        }

        public override IEnumerable<Output> Outputs()
        {
            yield return output;
        }

        public override void Generate(Chunk chunk, Biome currentBiome = null)
        {
            //getting input
            var src = (Matrix) input.GetObject(chunk);

            //return on stop/disable/null input
            if (chunk.stop || src == null) return;
            if (!enabled)
            {
                output.SetObject(chunk, src);
                return;
            }

            //preparing output
            var dst = src.Copy(null);

            for (var i = 0; i < dst.array.Length; i++)
            {
                var val = dst.array[i];
                dst.array[i] = Mathf.Clamp(val, Min, Max);
            }
            //mask and safe borders
            if (chunk.stop) return;
            var mask = (Matrix) maskIn.GetObject(chunk);
            if (mask != null) Matrix.Mask(src, dst, mask);

            //setting output
            if (chunk.stop) return;
            output.SetObject(chunk, dst);
        }

        public override void OnGUI()
        {
            //inouts
            layout.Par(20);
            input.DrawIcon(layout);
            output.DrawIcon(layout);
            layout.Par(20);
            maskIn.DrawIcon(layout);

            layout.Field(ref Min, "Min");
            layout.Field(ref Max, "Max");
        }
    }
}