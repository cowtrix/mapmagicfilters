using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MapMagic
{
    [System.Serializable]
    [GeneratorMenu(menu = "", name = "Additive Portal", disengageable = true)]
    public class AdditivePortal : Generator
    {
        public Input[] inputArray = 
        {
            new Input("Input", InoutType.Map, write: false, mandatory: true)
        };
        public Output output = new Output("Output", InoutType.Map);

        private int connectedCount = 0;

        public override IEnumerable<Input> Inputs()
        {
            if (inputArray == null)
            {
                yield break;
            }
            for (int i = 0; i < inputArray.Length; i++)
            {
                yield return  inputArray[i];
            }
        }

        public override IEnumerable<Output> Outputs() { yield return output; }

        public InoutType type;
        public enum PortalForm { In, Out }
        public PortalForm form;

        public string Key;
        public int InputCount = 1;

        public override void Generate(Chunk chunk, Biome generatingBiome = null)
        {
            if (form == PortalForm.Out)
            {
                object obj = null;

                // Find input list
                var otherAdditivePortals = MapMagic.instance.gens.GeneratorsOfType<AdditivePortal>();
                var inputEnumerator = otherAdditivePortals.GetEnumerator();
                List<Input> inputList = new List<Input>();
                while (inputEnumerator.MoveNext())
                {
                    var otherAdditivePortal = inputEnumerator.Current;
                    if (otherAdditivePortal.Key == Key 
                        && otherAdditivePortal.form == PortalForm.In
                        && otherAdditivePortal.enabled)
                    {
                        foreach (var input1 in otherAdditivePortal.Inputs())
                        {
                            inputList.Add(input1);
                        }
                    }
                }
                //GenState = EGeneratorState.Normal;

                // Do input
                lock (inputList)
                {
                    connectedCount = 0;
                    var inputListEnumerator = inputList.GetEnumerator();
                    while (inputListEnumerator.MoveNext())
                    {
                        var input = inputListEnumerator.Current;
                        var inputObj = input.GetObject(chunk);

                        if (inputObj == null)
                        {
                            GenState = EGeneratorState.ExceptionThrown;
                            //Debug.LogError("Input obj was null!");
                            continue;
                        }

                        connectedCount++;

                        if (type == InoutType.Objects)
                        {
                            var addMatrix = inputObj as SpatialHash;
                            if (addMatrix == null)
                            {
                                continue;
                            }
                            if (obj == null)
                            {
                                obj = addMatrix.Copy();
                                continue;
                            }
                            var baseMatrix = obj as SpatialHash;
                            baseMatrix.Add(addMatrix);
                        }
                        else if (type == InoutType.Map)
                        {
                            var addMatrix = inputObj as Matrix;
                            if (addMatrix == null)
                            {
                                continue;
                            }
                            if (obj == null)
                            {
                                obj = addMatrix.Copy();
                                continue;
                            }
                            var baseMatrix = obj as Matrix;
                            baseMatrix.Add(addMatrix);
                        }
                    }
                }
                
                if (chunk.stop) return;
                output.SetObject(chunk, obj);
            }
        }

        public override void GenerateWithPriors(Chunk tw, Biome biome = null)
        {
            if (tw.stop || !enabled)
            {
                return;
            }
            if (form == PortalForm.Out)
            {
                var otherAdditivePortals = MapMagic.instance.gens.GeneratorsOfType<AdditivePortal>();
                foreach (var otherAdditivePortal in otherAdditivePortals)
                {
                    if (otherAdditivePortal != null && otherAdditivePortal.enabled && otherAdditivePortal.form == PortalForm.In && otherAdditivePortal.Key == Key)
                    {
                        try
                        {
                            otherAdditivePortal.GenerateWithPriors(tw, biome);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError(string.Format("Generate Thread Error on layer {0}:\n{1}", otherAdditivePortal.UILayer, e));
                            otherAdditivePortal.GenState = EGeneratorState.ExceptionThrown;
                        }
                    }
                }
            }
            base.GenerateWithPriors(tw);
        }

        public override bool IsDependentFrom(Generator prior)
        {
            if (form == PortalForm.Out)
            {
                var gens = MapMagic.instance.gens.GeneratorsOfType<AdditivePortal>();
                foreach (var gen in gens)
                {
                    if (gen == null || !gen.enabled)
                    {
                        continue;
                    }
                    if (gen.Key == Key && gen == prior && gen.form == PortalForm.In)
                    {
                        return true;
                    }
                    return false;
                }
            }
            return base.IsDependentFrom(prior);
        }

        public override void OnGUI()
        {
            InputCount = Math.Max(InputCount, 1);
            if (inputArray == null)
            {
                inputArray = new[]
                {
                    new Input("Input", InoutType.Map, write: false, mandatory: true)
                };
            }
            while (InputCount > inputArray.Length)
            {
                ArrayTools.Add(ref inputArray, new Input("Input", type, write: false, mandatory: false));
            }
            while (InputCount < inputArray.Length)
            {
                ArrayTools.RemoveAt(ref inputArray, inputArray.Length - 1);
            }
            
            layout.margin = 18;
            layout.rightMargin = 15;
            if (form == PortalForm.Out)
            {
                output.DrawIcon(layout);
            }

            layout.Field(ref type);
            if (type != inputArray[0].type)
            {
                for (int i = 0; i < inputArray.Length; i++)
                {
                    var input1 = inputArray[i];
                    input1.Unlink();
                    input1.type = type; 
                }
                output.type = type;
            }

            layout.Field(ref form);
            layout.Field(ref Key);
            
            if(form == PortalForm.In)
            {
                layout.Field(ref InputCount, "Input Count");
                for (int i = 0; i < inputArray.Length; i++)
                {
                    layout.Par(20);
                    inputArray[i].DrawIcon(layout);
                }
            }
            else
            {
                layout.Label(string.Format("Ins: {0}", connectedCount));
            }
        }
    }
}