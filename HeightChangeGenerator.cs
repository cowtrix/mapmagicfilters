using System;
using System.Collections.Generic;

namespace MapMagic
{
    [Serializable]
    [GeneratorMenu(menu = "CustomObjects", name = "Height", disengageable = true)]
    public class HeightChangeGenerator : Generator
    {
        public float height;
        public Input input = new Input("Input", InoutType.Objects, false, true);
        public Input intensity = new Input("Mask", InoutType.Map, false);
        public Output output = new Output("Output", InoutType.Objects);
        public int seed = 12345;
        public float sizeFactor;

        public override IEnumerable<Input> Inputs()
        {
            yield return input;
            yield return intensity;
        }

        public override IEnumerable<Output> Outputs()
        {
            yield return output;
        }

        public override void Generate(MapMagic.Chunk chunk)
        {
            //getting inputs
            var sourceHash = (SpatialHash) input.GetObject(chunk);
            if (sourceHash == null) return;
            var spatialHash = sourceHash.Copy();
            var intensityMatrix = (Matrix) intensity.GetObject(chunk);

            //return on stop/disable/null input
            if (chunk.stop || spatialHash == null) return;
            if (!enabled)
            {
                output.SetObject(chunk, spatialHash);
                return;
            }

            //preparing output
            spatialHash = spatialHash.Copy();

            var rnd = new InstanceRandom(MapMagic.instance.seed + seed + chunk.coord.x*1000 + chunk.coord.z, 1000);

            foreach (var obj in spatialHash.AllObjs())
            {
                float percent = 1;
                if (intensityMatrix != null) percent = intensityMatrix[obj.pos];

                //everything else does
                percent = percent*(1 - sizeFactor) + percent*obj.sizeScalar*sizeFactor;
                obj.height += height*percent/MapMagic.instance.terrainHeight;
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
            layout.Field(ref seed, "Seed");
            layout.Field(ref height, "Height");
            layout.Field(ref sizeFactor, "Size Factor");
        }
    }
}