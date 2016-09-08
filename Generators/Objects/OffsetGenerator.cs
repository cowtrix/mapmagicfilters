using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapMagic
{
    [Serializable]
    [GeneratorMenu(menu = "CustomObjects", name = "Offset", disengageable = true)]
    public class OffsetGenerator : Generator
    {
        public Input input = new Input("Input", InoutType.Objects, false, true);
        public Input intensity = new Input("Mask", InoutType.Map, false);
        public Output output = new Output("Output", InoutType.Objects);

        public float sizeFactor = 1;
        public float Distance;
        public float Angle = 0;

        public override IEnumerable<Input> Inputs()
        {
            yield return input;
            yield return intensity;
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
            var intensityMatrix = (Matrix)intensity.GetObject(chunk);

            //return on stop/disable/null input
            if (chunk.stop || spatialHash == null) return;
            if (!enabled)
            {
                output.SetObject(chunk, spatialHash);
                return;
            }

            //preparing output
            spatialHash = spatialHash.Copy();

            foreach (var obj in spatialHash.AllObjs())
            {
                float percent = 1;
                if (intensityMatrix != null) percent = intensityMatrix[obj.pos];

                //everything else does
                percent = percent * (1 - sizeFactor) + percent * obj.sizeScalar * sizeFactor;
                var offset = (Vector2)(obj.rotation * Quaternion.Euler(0, Angle, 0) * Vector3.forward * Distance * percent).xz();
                offset = Vector2.Scale(offset, obj.size.xz());
                obj.pos += offset;
            }

            //setting output
            if (chunk.stop) return;
            output.SetObject(chunk, spatialHash);
        }

        public override void OnGUI()
        {
            //inouts
            layout.Par(20);
            input.DrawIcon(layout);
            output.DrawIcon(layout);
            layout.Par(20);
            intensity.DrawIcon(layout);
            layout.Par(5);

            //params
            layout.fieldSize = 0.7f;
            //			layout.inputSize = 0.5f;
            layout.Field(ref Distance, "Distance");
            layout.Field(ref sizeFactor, "Size Factor");
            layout.Field(ref Angle, "Angle");
        }
    }
}