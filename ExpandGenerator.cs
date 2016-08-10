using System;
using System.Collections.Generic;

namespace MapMagic
{
    [Serializable]
    [GeneratorMenu(menu = "CustomMap", name = "Expand", disengageable = true)]
    public class ExpandGenerator : Generator
    {
        public Input input = new Input("Input", InoutType.Map, false); //, mandatory:true);
        public Input maskIn = new Input("Mask", InoutType.Map);
        public Output output = new Output("Output", InoutType.Map);
		
        public int size = 5;

        public override IEnumerable<Input> Inputs()
        {
            yield return input;
            yield return maskIn;
        }

        public override IEnumerable<Output> Outputs()
        {
            yield return output;
        }

        public override void Generate(MapMagic.Chunk chunk)
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

            for (var x = src.rect.Min.x; x < src.rect.Max.x; x++)
            {
                for (var z = src.rect.Min.z; z < src.rect.Max.z; z++)
                {
                    for (var u = -size; u < size; ++u)
                    {
                        for (var v = -size; v < size; ++v)
                        {
                            try
                            {
								dst[x + u, z + v] += src[x, z];
                            }
                            catch (IndexOutOfRangeException)
                            {
                            }
                        }
                    }
                }
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

            layout.Field(ref size, "Size");
        }
    }
}