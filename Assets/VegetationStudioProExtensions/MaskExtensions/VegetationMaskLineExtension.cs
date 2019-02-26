using UnityEngine;
using AwesomeTechnologies;

namespace VegetationStudioProExtensions
{
    [RequireComponent(typeof(VegetationMaskLine))]
    public class VegetationMaskLineExtension : MonoBehaviour
    {
        /// <summary>
        /// A gameobject with children. These children will be used to create the line mask.
        /// </summary>
        [HideInInspector]
        public GameObject container;
    }
}
