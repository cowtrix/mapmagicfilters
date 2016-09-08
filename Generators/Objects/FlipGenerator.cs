using System.Collections.Generic;
using UnityEngine;

namespace MapMagic
{
    [System.Serializable]
    [GeneratorMenu(menu = "CustomObjects", name = "Flip", disengageable = true)]
    public class FlipGenerator : Generator
    {
        public Input input = new Input("Input", InoutType.Objects, write: false, mandatory: true);
        public Output output = new Output("Output", InoutType.Objects);
        public override IEnumerable<Input> Inputs() { yield return input; }
        public override IEnumerable<Output> Outputs() { yield return output; }

        public bool flipX = true;
        public bool flipY = true;
        public bool flipZ = true;

        public override void Generate(Chunk chunk, Biome generatingBiome = null)
        {
            //getting inputs
            SpatialHash sourceHash = (SpatialHash)input.GetObject(chunk); if (sourceHash == null) return;
            SpatialHash spatialHash = sourceHash.Copy();

            //return on stop/disable/null input
            if (chunk.stop || spatialHash == null) return;
            if (!enabled) { output.SetObject(chunk, spatialHash); return; }

            //preparing output
            spatialHash = spatialHash.Copy();

            InstanceRandom rnd = new InstanceRandom(MapMagic.instance.seed + seed + chunk.coord.x * 1000 + chunk.coord.z, lutLength: 1000);

            foreach (SpatialObject obj in spatialHash.AllObjs())
            {
                var mag = obj.size.magnitude;
                if (flipX && rnd.Random() < 0.5f)
                {
                    obj.size = new Vector3(-obj.size.x, obj.size.y, obj.size.z);
                }
                if (flipY && rnd.Random() < 0.5f)
                {
                    obj.size = new Vector3(obj.size.x, -obj.size.y, obj.size.z);
                }
                if (rnd.Random() < 0.5f && flipZ)
                {
                    obj.size = new Vector3(obj.size.x, obj.size.y, -obj.size.z);
                }
                if (mag != obj.size.magnitude)
                {
                    Debug.LogError("Y");
                }
            }

            //setting output
            if (chunk.stop) return;
            output.SetObject(chunk, spatialHash);
        }

        public override void OnGUI()
        {
            //inouts
            layout.Par(20); input.DrawIcon(layout); output.DrawIcon(layout);
            layout.Par(5);

            //params
            layout.fieldSize = 0.7f;
            //			layout.inputSize = 0.5f;
            layout.Field(ref seed, "Seed");
            layout.Field(ref flipX, "Flip X");
            layout.Field(ref flipY, "Flip Y");
            layout.Field(ref flipZ, "Flip Z");
        }
    }
}