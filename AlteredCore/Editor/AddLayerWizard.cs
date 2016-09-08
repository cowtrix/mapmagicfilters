using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MapMagic
{
    public class AddLayerWizard : ScriptableWizard
    {
        public string LayerName = "New Layer";

        void OnWizardCreate()
        {
            //Debug.Log("Added layer " + LayerName);
            MapMagic.instance.guiGens.LayerNames.Add(new GUIContent(LayerName));
        }
    }

    public class RenameLayerWizard : ScriptableWizard
    {
        public List<GUIContent> LayerNames;

        void OnWizardCreate()
        {
            if (LayerNames.Count != MapMagic.instance.guiGens.LayerNames.Count)
            {
                Debug.LogError("Can't remove or add layers like this!");
                return;
            }

            for (int i = 0; i < LayerNames.Count; i++)
            {
                var layerName = LayerNames[i];
                MapMagic.instance.guiGens.LayerNames[i] = layerName;
            }
        }
    }
}