using UnityEngine;
using UnityEditor;

namespace VegetationStudioProExtensions
{
    public class MenuExtension : MonoBehaviour
    {
        // Add a menu item to create custom GameObjects.
        // Priority 1 ensures it is grouped with the other menu items of the same kind
        // and propagated to the hierarchy dropdown and hierarchy context menus.
        [MenuItem("GameObject/VegetationStudioPro/Create Biome Mask Area", false, 10)]
        static void CreateBiomeMaskArea(MenuCommand menuCommand)
        {
            // create new gameobject
            GameObject go = new GameObject("Biome Mask Area");

            // add this component
            go.AddComponent<BiomeMaskAreaExtension>();

            // position it to the center of the viewport
            SceneView.lastActiveSceneView.MoveToView(go.transform);

            // ensure gameobject gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

            // tegister the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

            Selection.activeObject = go;

        }

        [MenuItem("GameObject/VegetationStudioPro/Create Vegetation Mask Area", false, 10)]
        static void CreateVegetationMaskArea(MenuCommand menuCommand)
        {
            // create new gameobject
            GameObject go = new GameObject("Vegetation Mask Area");

            // add this component
            go.AddComponent<VegetationMaskAreaExtension>();

            // position it to the center of the viewport
            SceneView.lastActiveSceneView.MoveToView(go.transform);

            // ensure gameobject gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

            // tegister the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

            Selection.activeObject = go;

        }


    }
}