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

        public override void Generate(Chunk chunk, Biome generatingBiome = null)
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
                var firstEnum = src.AllObjs().GetEnumerator();
                while (firstEnum.MoveNext())
                {
                    var first = firstEnum.Current;
                    var secondEnum = src.AllObjs().GetEnumerator();
                    while (secondEnum.MoveNext())
                    {
                        var second = secondEnum.Current;
                        if (first == second)
                        {
                            continue;
                        }
                        if (toRemove.Contains(first) || toRemove.Contains(second))
                        {
                            continue;
                        }

                        var tHeight = MapMagic.instance.terrainHeight;
                        var firstPos = new Vector3(first.pos.x, first.height * tHeight, first.pos.y);
                        var secondPos = new Vector3(second.pos.x, second.height * tHeight, second.pos.y);

                        if (Vector3.Distance(firstPos, secondPos) < Size * first.sizeScalar * second.sizeScalar)
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

            layout.Field(ref Size, "Size");
        }
    }
}