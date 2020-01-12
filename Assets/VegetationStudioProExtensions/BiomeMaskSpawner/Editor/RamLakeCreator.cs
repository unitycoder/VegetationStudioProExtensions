using AwesomeTechnologies.VegetationSystem.Biomes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VegetationStudioProExtensions
{
    /// <summary>
    /// Create a lake for RAM 2019. 
    /// Requires the scripting define symbol RAM_2019 to be set.
    /// </summary>
    public class RamLakeCreator : MonoBehaviour
    {

#if RAM_2019

        /// <summary>
        /// Create a lake and re-parent the mask so that the lake becomes the child of the container and the mask the child of the lake.
        /// 
        /// Note: the lake uses a bezier curve on the points, so the lake most likely won't be 100% in line with the mask.
        /// </summary>
        /// <param name="maskGameObject"></param>
        /// <param name="gameObjectName"></param>
        /// <param name="nodes"></param>
        public static LakePolygon CreateLakePolygon( LakeSettings lakeSettings, BiomeMaskArea mask, GameObject maskGameObject, string gameObjectName, List<Vector3> nodes)
        {
            GameObject maskParentGameObject = maskGameObject.transform.parent.gameObject;
            Vector3 maskPosition = maskGameObject.transform.position;

            // lake
            LakePolygon lakePolygon = LakePolygon.CreatePolygon(AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat"));

            // apply profile
            lakePolygon.currentProfile = lakeSettings.lakeProfile;

            // apply gameobject data
            lakePolygon.transform.localPosition = Vector3.zero;
            lakePolygon.name = "Lake " + gameObjectName;

            // add biome nodes
            if (lakeSettings.ramInternalLakeCreation)
            {
                // apply settings
                lakePolygon.angleSimulation = lakeSettings.angleSimulation;
                lakePolygon.closeDistanceSimulation = lakeSettings.closeDistanceSimulation;
                lakePolygon.checkDistanceSimulation = lakeSettings.checkDistanceSimulation;
                lakePolygon.removeFirstPointSimulation = lakeSettings.removeFirstPointSimulation;

                // add point
                lakePolygon.AddPoint(maskPosition);

                // start simulation
                lakePolygon.Simulation();

            }
            // use mask shape
            else
            {
                foreach (Vector3 node in nodes)
                {
                    lakePolygon.AddPoint(maskParentGameObject.transform.InverseTransformPoint(node));
                }
            }


            // generate the lake
            lakePolygon.GeneratePolygon();

            // re-parent the mask to the lake and the lake to the parent
            GameObjectUtility.SetParentAndAlign(lakePolygon.gameObject, maskParentGameObject);
            GameObjectUtility.SetParentAndAlign(maskGameObject, lakePolygon.gameObject);

            // adjust the lake position
            lakePolygon.transform.position = maskPosition;

            // reset the mask position, it's now a child of the lake
            if (lakeSettings.ramInternalLakeCreation)
            {
                maskGameObject.transform.position = Vector3.zero;

                List<Vector3> newPoints = new List<Vector3>();

                foreach (Vector3 node in lakePolygon.points)
                {
                    newPoints.Add(lakePolygon.transform.TransformPoint(node));
                }

                // set the lake's polygon as new mask nodes
                SetMaskNodes(mask, newPoints);

                // put move handle into the center of the polygon
                BiomeMaskUtils.CenterMainHandle(mask);

            }

            // re-apply the material so that the water becomes immediately visible
            MeshRenderer meshRenderer = lakePolygon.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = lakePolygon.currentProfile.lakeMaterial;

            /* carving is disabled for now
            if ( extension.lakeSettings.carveTerrain)
            {
                Debug.Log("Start carve");
                lakePolygon.terrainSmoothMultiplier = 2f;
                lakePolygon.distSmooth = 10f;
                lakePolygon.TerrainCarve();
            }
            */

            // EditorUtility.SetDirty(lakePolygon);

            return lakePolygon;
        }

        /// <summary>
        /// Clear the nodes of the mask and set the provided ones.
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="nodes"></param>
        private static void SetMaskNodes(BiomeMaskArea mask, List<Vector3> nodes)
        {
            mask.ClearNodes();

            foreach (Vector3 node in nodes)
            {
                mask.AddNodeToEnd(node);
            }

            mask.PositionNodes();
        }

#endif
    }
}