using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VegetationStudioProExtensions;

public class TrainController
{
    VegetationMaskLineExtension editorTarget;

    public TrainController(VegetationMaskLineExtension editorTarget)
    {
        this.editorTarget = editorTarget;
    }

    /// <summary>
    /// Positions are determined by the spline positions of Train Controller.
    /// </summary>
    /// <returns></returns>
    public List<Vector3> GetTrainControllerPositions()
    {
        List<Vector3> positions = new List<Vector3>();

#if TRAIN_CONTROLLER

        WSMGameStudio.Splines.Spline spline = editorTarget.dataSource.GetComponent<WSMGameStudio.Splines.Spline>();

        if (spline)
        {
            int steps = WSMGameStudio.Splines.SplineDefaultValues.StepsPerCurve * spline.CurveCount;

            for (int i = 0; i <= steps; i++)
            {
                float t;

                if (i == 0)
                {
                    t = 0f;
                }
                else
                {
                    t = i / (float)steps;
                }

                Vector3 position = spline.GetPoint(t);

                positions.Add(position);
            }
        }
#else
            Debug.LogError("Train Controller selected, but scripting define symbol TRAIN_CONTROLLER isn't set");
#endif

        return positions;
    }

}
