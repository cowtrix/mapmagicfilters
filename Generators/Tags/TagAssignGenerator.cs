using System.Collections.Generic;
using UnityEngine;

namespace MapMagic
{
    [System.Serializable]
    [GeneratorMenu(menu = "Tags", name = "Assign Tag", disengageable = true)]
    public class TagAssignGenerator : Generator, Layout.ILayered
    {
        //layer
        public class Layer : Layout.ILayer
        {
            public Output output = new Output("Object Layer", InoutType.Objects);
            public float chance = 1;
            public string Tag;

            public bool pinned { get { return false; } }

            public void OnCollapsedGUI(Layout layout)
            {
                layout.rightMargin = 20; layout.fieldSize = 1f;
                layout.Par(20);
                var dispName = Tag;
                if (string.IsNullOrEmpty(dispName))
                {
                    dispName = "Undefined";
                }
                layout.Label(dispName, rect: layout.Inset());
                output.DrawIcon(layout);
            }

            public void OnExtendedGUI(Layout layout)
            {
                layout.margin = 7; layout.rightMargin = 20; layout.fieldSize = 1f;

                //layout.Par(20);
                //output.guiName = layout.Field(output.guiName, rect: layout.Inset());
                output.DrawIcon(layout);

                layout.margin = 5; layout.rightMargin = 5; layout.fieldSize = 0.6f;

                layout.Field(ref Tag, "Tag");
                layout.Field(ref chance, "Chance");
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

        //params
        public enum MatchType { layered, random };
        public MatchType matchType = MatchType.layered;

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

            //random
            InstanceRandom rnd = new InstanceRandom(MapMagic.instance.seed + 12345 + chunk.coord.x * 1000 + chunk.coord.z);

            //procedural array
            bool[] match = new bool[baseLayers.Length];

            //for each object
            foreach (SpatialObject obj in src.AllObjs())
            {
                //finding suitable objects (and sum of chances btw. And last object for non-random)
                int matchesNum = 0; //how many layers have a suitable obj
                float chanceSum = 0;
                int lastLayerNum = 0;

                for (int i = 0; i < baseLayers.Length; i++)
                {
                    Layer layer = baseLayers[i];
                    match[i] = true;
                    matchesNum++;
                    chanceSum += layer.chance;
                    lastLayerNum = i;
                }

                //if no matches detected - continue withous assigning obj
                if (matchesNum == 0) continue;

                //if one match - assigning last obj
                else if (matchesNum == 1 || matchType == MatchType.layered) dst[lastLayerNum].Add(obj);

                //selecting layer at random
                else if (matchesNum > 1 && matchType == MatchType.random)
                {
                    float randomVal = rnd.CoordinateRandom(obj.id);
                    randomVal *= chanceSum;
                    chanceSum = 0;

                    for (int i = 0; i < baseLayers.Length; i++)
                    {
                        if (!match[i]) continue;

                        Layer layer = baseLayers[i];
                        if (randomVal > chanceSum && randomVal < chanceSum + layer.chance)
                        {
                            var tag = new StringTuple(Key, layer.Tag);
                            obj.Tags.Remove(tag);
                            obj.Tags.Add(tag);
                            dst[i].Add(obj); break;
                        }
                        chanceSum += layer.chance;
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
            layout.Par(5);

            //params
            layout.Par();
            layout.Label("Match Type", rect: layout.Inset(0.5f));
            layout.Field(ref matchType, rect: layout.Inset(0.5f));

            layout.Field(ref Key, "Key");

            layout.DrawLayered(this, "Layers:");
        }
    }
}
