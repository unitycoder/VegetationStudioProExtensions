Settings Template
-------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    [System.Serializable]
    public class RoadSettings
    {
    }
}


Module Template
-------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    public class RoadModule
    {

        private BiomeMaskSpawnerExtensionEditor editor;

        public RoadModule(BiomeMaskSpawnerExtensionEditor editor)
        {
            this.editor = editor;
        }

        public void OnEnable()
        {
        }

        public void OnInspectorGUI()
        {
        }
    }
}
