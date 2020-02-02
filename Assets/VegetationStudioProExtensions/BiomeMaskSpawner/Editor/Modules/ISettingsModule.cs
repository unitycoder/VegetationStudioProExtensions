using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    public interface ISettingsModule
    {
        void OnEnable();
        void OnInspectorGUI();
    }
}

