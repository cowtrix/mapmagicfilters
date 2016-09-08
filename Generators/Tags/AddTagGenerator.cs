using System.Collections.Generic;

namespace MapMagic
{
    [System.Serializable]
    [GeneratorMenu(menu = "Tags", name = "Add Tag", disengageable = true)]
    public class AddTagGenerator : Generator
    {
        public Input input = new Input("Input", InoutType.Objects, write: false, mandatory: true);
        public Output output = new Output("Output", InoutType.Objects);
        public override IEnumerable<Input> Inputs() { yield return input; }
        public override IEnumerable<Output> Outputs() { yield return output; }

        public string Key;
        public string Value;

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

            foreach (SpatialObject obj in spatialHash.AllObjs())
            {
                var newTag = new StringTuple(Key, Value);
                if (!obj.Tags.Contains(newTag))
                {
                    obj.Tags.Add(newTag);
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
            layout.Field(ref Key);
            layout.Field(ref Value);
        }
    }
}