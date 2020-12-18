using UnityEngine;
using AwesomeTechnologies;

namespace VegetationStudioProExtensions
{
    [RequireComponent(typeof(VegetationMaskLine))]
    public class VegetationMaskLineExtension : MonoBehaviour
    {
        public enum DataSourceType
        {
            Container,
            TrainController
        }

        public DataSourceType dataSourceType = DataSourceType.Container;

        /// <summary>
        /// A gameobject with children. These children will be used to create the line mask
        /// </summary>
        public GameObject dataSource;

        /// <summary>
        /// If Closed Path is selected, then the last point will be connected to the first point
        /// </summary>
        public bool closedPath;


        /// <summary>
        /// If this value is greater than 0, then a Dougles Peucker algorithm will be used with this tolerance level in order to reduce the number of points.
        /// </summary>
        public float douglasPeuckerReductionTolerance = 0f;

    }
}
