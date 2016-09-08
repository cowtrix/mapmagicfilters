using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MapMagic
{
    public enum EGeneratorState
    {
        Normal,
        ExceptionThrown,
        CurrentlyProcessing,
        Pending,
    }
    
    public abstract partial class Generator
    {
        public int seed;
        public int UILayer = 0;
        public EGeneratorState GenState = EGeneratorState.Normal;

        public Generator()
        {
            var rnd = new System.Random();
            seed = Mathf.RoundToInt(rnd.Next());
        }

        public virtual void TrySetLayer(int index, bool force = false)
        {
            var allGens = MapMagic.instance.gens.list;
            bool anyLinks = false;

            if (!force)
            {
                foreach (var input in Inputs())
                {
                    if (input != null && input.link != null)
                    {
                        anyLinks = true;
                    }
                }
                foreach (var output in Outputs())
                {
                    if (output != null && output.GetConnectedInputs(allGens) != null)
                    {
                        anyLinks = true;
                    }
                }
            }

#if UNITY_EDITOR
            if (anyLinks)
            {
                if (
                    !UnityEditor.EditorUtility.DisplayDialog("Change layer recursively?",
                        "This will change the layers of all connected generators. Continue?", "Continue", "Cancel"))
                {
                    return;
                }
            }
#endif

            UILayer = index;

            if (this is Portal)
            {
                var pRef = this as Portal;
                if (pRef.form == Portal.PortalForm.In)
                {
                    SetLayerInputsRecursive(index, allGens);
                }
                else
                {
                    SetLayerOutputsRecursive(index, allGens);
                }
            }
            else
            {
                SetLayerInputsRecursive(index, allGens);
                SetLayerOutputsRecursive(index, allGens);
            }
        }

        private void SetLayerOutputsRecursive(int index, Generator[] allGens)
        {
            foreach (var output in Outputs())
            {
                var next = output.GetConnectedInputs(allGens);
                for (int i = 0; i < next.Count; i++)
                {
                    var input = next[i];
                    if (input != null)
                    {
                        input.GetGenerator(allGens).TrySetLayer(index, true);
                    }
                }
            }
        }

        private void SetLayerInputsRecursive(int index, Generator[] allGens)
        {
            foreach (var input in Inputs())
            {
                var link = input.link;
                if (link != null)
                {
                    var gen = link.GetGenerator(allGens);
                    if (gen.UILayer != index)
                    {
                        gen.TrySetLayer(index, true);
                    }
                }
            }
        }

        public partial class Input
        {
            public string guiName;
        }

        public partial class Output
        {
            public List<Input> GetConnectedInputs(Generator[] gens)
            {
                var ret = new List<Input>();
                for (int g = 0; g < gens.Length; g++)
                    foreach (Input input in gens[g].Inputs())
                        if (input.link == this) ret.Add(input);
                return ret;
            }
        }
    }

    public partial class Group
    {
        public override void TrySetLayer(int index, bool force = false)
        {
            Populate(MapMagic.instance.gens);
            foreach (var generator in generators)
            {
                generator.TrySetLayer(index, true);
            }
            base.TrySetLayer(index, force);
        }
    }
}