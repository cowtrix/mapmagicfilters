using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapMagic
{
    [Serializable]
    [GeneratorMenu(menu = "CustomObjects", name = "Remove Overlap", disengageable = true)]
    public class RemoveOverlappingGenerator : Generator
    {
        public Input input = new Input("Input", InoutType.Objects);
        public Input mask = new Input("Mask", InoutType.Map, false);
        public Output output = new Output("Output", InoutType.Objects);
        public int seed = 12345;
        public float Size = 1f;

        public override IEnumerable<Input> Inputs()
        {
            yield return mask;
            yield return input;
        }

        public override IEnumerable<Output> Outputs()
        {
            yield return output;
        }

        public override void Generate(MapMagic.Chunk chunk)
        {
            var src = (SpatialHash) input.GetObject(chunk);
            if (!enabled)
            {
                output.SetObject(chunk, src);
                return;
            }
            if (chunk.stop || src == null) return;

            var toRemove = new HashSet<SpatialObject>();
            if (src.AllObjs() != null)
            {
                foreach (var first in src.AllObjs())
                {
                    foreach (var second in src.AllObjs())
                    {
                        if (first == second)
                        {
                            continue;
                        }
                        if (toRemove.Contains(first) || toRemove.Contains(second))
                        {
                            continue;
                        }
                        if (Vector2.Distance(first.pos, second.pos) < Size*first.sizeScalar*second.sizeScalar)
                        {
                            toRemove.Add(second);
                        }
                    }
                }
            }

            //preparing output
            var dst = new SpatialHash(src.offset, src.size, src.resolution);

            //populating output
            foreach (var obj in src.AllObjs())
                if (!toRemove.Contains(obj)) dst.Add(obj);

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
            mask.DrawIcon(layout);
            layout.Par(5);

            layout.Field(ref seed, "Seed");
            layout.Field(ref Size, "Size");
        }
    }
}