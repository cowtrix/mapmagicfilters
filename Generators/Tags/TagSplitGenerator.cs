using System.Collections.Generic;

namespace MapMagic
{
    [System.Serializable]
    [GeneratorMenu(menu = "Tags", name = "Split By Tag", disengageable = true)]
    public class TagSplitGenerator : Generator, Layout.ILayered
    {
        //layer
        public class Layer : Layout.ILayer
        {
            public Output output = new Output("Object Layer", InoutType.Objects);
            public string Value;

            public bool pinned { get { return false; } }

            public void OnCollapsedGUI(Layout layout)
            {
                layout.rightMargin = 20; layout.fieldSize = 1f;

                layout.Par(20);
                layout.Label(Value, rect: layout.Inset());
                output.DrawIcon(layout);
            }

            public void OnExtendedGUI(Layout layout)
            {
                layout.margin = 7; layout.rightMargin = 20; layout.fieldSize = 1f;

                //layout.Par(20);
                layout.Field(ref Value, "Value");
                output.DrawIcon(layout);
            }

            public void OnAdd() { }
            public void OnRemove()
            {
                Input connectedInput = output.GetConnectedInput(MapMagic.instance.gens.list);
                if (connectedInput != null) connectedInput.Link(null, null);
            }
        }

        public Layer[] baseLayers = new Layer[0];
        
        public Layout.ILayer[] layers
        {
            get { return baseLayers; }
            set { baseLayers = ArrayTools.Convert<Layer, Layout.ILayer>(value); }
        }

        public int selected { get; set; }
        public int collapsedHeight { get; set; }
        public int extendedHeight { get; set; }
        public Layout.ILayer def { get { return new Layer(); } }

        //generator
        public Input input = new Input("Input", InoutType.Objects, write: false, mandatory: true);
        public override IEnumerable<Input> Inputs() { yield return input; }
        public override IEnumerable<Output> Outputs()
        {
            for (int i = 0; i < baseLayers.Length; i++)
                yield return baseLayers[i].output;
        }

        public string Key = "SpawnPrefab";

        public override void Generate(Chunk chunk, Biome generatingBiome)
        {
            //getting input
            SpatialHash src = (SpatialHash)input.GetObject(chunk);

            //return on stop/disable/null input
            if (chunk.stop || !enabled || src == null) return;

            //creating dst
            SpatialHash[] dst = new SpatialHash[baseLayers.Length];
            for (int i = 0; i < dst.Length; i++)
                dst[i] = new SpatialHash(src.offset, src.size, src.resolution);

            //for each object
            foreach (SpatialObject obj in src.AllObjs())
            {
                for (int i = 0; i < baseLayers.Length; i++)
                {
                    var baseLayer = baseLayers[i];
                    var key = obj.Tags.Find(tuple => tuple.Key == Key);
                    if (obj.Tags.Contains(key))
                    {
                        if (key.Value == baseLayer.Value)
                        {
                            dst[i].Add(obj);
                        }
                    }
                }
            }

            for (int i = 0; i < baseLayers.Length; i++)
                baseLayers[i].output.SetObject(chunk, dst[i]);
        }

        public override void OnGUI()
        {
            //inouts
            layout.Par(20); input.DrawIcon(layout);
            layout.Field(ref Key, "Key");
            layout.DrawLayered(this, "Layers:");
        }
    }
}