using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MapMagic
{
    public partial class MapMagicWindow
    {
        private int _currentLayerIndex = 0;

        private void ClearAll()
        {
            if (!EditorUtility.DisplayDialog("Really Clear All Nodes?", "This will delete all nodes and layers!", "Yes",
                "No"))
            {
                return;
            }
            MapMagic.instance.gens.ClearGenerators();
            //MapMagic.instance.guiGens.ClearGenerators();
            //MapMagic.instance.gens.LayerNames = new List<GUIContent>() { new GUIContent("Default") };

            _currentLayerIndex = 0;
        }

        public List<GUIContent> GetLayerNames
        {
            get { return MapMagic.instance.guiGens.LayerNames; }
        }
    }
}
