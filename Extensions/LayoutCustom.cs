using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MapMagic
{
    public partial class Layout
    {
        bool DoCustomTypes(Type type, Rect fieldRect, object dstObj, object srcObj)
        {
#if UNITY_EDITOR
            if (type == typeof(LayerMask))
            {
                dstObj = LayerMaskFieldUtility.LayerMaskField(this.ToDisplay(fieldRect), (LayerMask)srcObj, false);
            }
#endif
            return false;
        }



        public static class LayerMaskFieldUtility
        {
            public static List<string> layers;
            public static List<int> layerNumbers;
            public static string[] layerNames;
            public static long lastUpdateTick;

            public static LayerMask LayerMaskField(Rect rect, LayerMask selected, bool showSpecial)
            {
#if UNITY_EDITOR
                //Unity 3.5 and up

                if (layers == null || (System.DateTime.Now.Ticks - lastUpdateTick > 10000000L && Event.current.type == EventType.Layout))
                {
                    lastUpdateTick = System.DateTime.Now.Ticks;
                    if (layers == null)
                    {
                        layers = new List<string>();
                        layerNumbers = new List<int>();
                        layerNames = new string[4];
                    }
                    else
                    {
                        layers.Clear();
                        layerNumbers.Clear();
                    }

                    int emptyLayers = 0;
                    for (int i = 0; i < 32; i++)
                    {
                        string layerName = LayerMask.LayerToName(i);

                        if (layerName != "")
                        {

                            for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i - emptyLayers));
                            layerNumbers.Add(i);
                            layers.Add(layerName);
                        }
                        else
                        {
                            emptyLayers++;
                        }
                    }

                    if (layerNames.Length != layers.Count)
                    {
                        layerNames = new string[layers.Count];
                    }
                    for (int i = 0; i < layerNames.Length; i++) layerNames[i] = layers[i];
                }

                selected.value = UnityEditor.EditorGUI.MaskField(rect, selected.value, layerNames);

                return selected;
#else
                return new LayerMask();
#endif
            }
        }

    }


}
