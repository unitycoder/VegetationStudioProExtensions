using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    public class VegetationStudioProUtils
    {
        public static void RefreshVegetation()
        {
            VegetationStudioManager VegetationStudioInstance = FindVegetationStudioInstance();

            List<VegetationSystemPro> VegetationSystemList = VegetationStudioInstance.VegetationSystemList;
            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                VegetationSystemPro vegetationSystemPro = VegetationSystemList[i];

                vegetationSystemPro.ClearCache();
                vegetationSystemPro.RefreshTerrainHeightmap();
                SceneView.RepaintAll();

                SetSceneDirty(vegetationSystemPro);
            }
        }

        public static void SetSceneDirty(VegetationSystemPro vegetationSystemPro)
        {
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(vegetationSystemPro.gameObject.scene);
                EditorUtility.SetDirty(vegetationSystemPro);
            }
        }

        public static VegetationStudioManager FindVegetationStudioInstance()
        {
            return (VegetationStudioManager)Object.FindObjectOfType(typeof(VegetationStudioManager));
        }

        /// <summary>
        /// Get the added biome types, i. e. only those which have an added vegetation package
        /// </summary>
        /// <param name="vegetationSystemPro"></param>
        /// <returns></returns>
        public static List<BiomeType> GetAddedBiomeTypes()
        {
            List<BiomeType> biomeTypes = new List<BiomeType>();

            VegetationStudioManager VegetationStudioInstance = FindVegetationStudioInstance();

            List<VegetationSystemPro> VegetationSystemList = VegetationStudioInstance.VegetationSystemList;

            for (int i = 0; i <= VegetationSystemList.Count - 1; i++)
            {
                VegetationSystemPro vegetationSystemPro = VegetationSystemList[i];

                foreach (VegetationPackagePro vegetationPackagePro in vegetationSystemPro.VegetationPackageProList)
                {
                    biomeTypes.Add(vegetationPackagePro.BiomeType);
                }
            }

            return biomeTypes;
        }
    }
}
