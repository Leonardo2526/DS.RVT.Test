using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class PointsCheker
    {
        public bool IsPointPassable(StepPoint point)
        {
            if (InputData.UnpassStepPoints.Count == 0)
                return true;

            for (int i = 0; i < InputData.UnpassLocX.Count; i++)
            {
                if (InputData.UnpassLocX[i] == point.X &&
                    InputData.UnpassLocY[i] == point.Y &&
                    InputData.UnpassLocZ[i] == point.Z)
                    return false;
            }

            return true;
        }

        public bool IsClearanceZoneAvailable(StepPoint stepPoint,
         List<StepPoint> clearancePoints, List<StepPoint> unpassableByCLZPoints)
        {
            if (unpassableByCLZPoints.Count == 0)
                return true;

            foreach (StepPoint clearancePoint in clearancePoints)
            {
                StepPoint currentPoint = new StepPoint(
                    stepPoint.X + clearancePoint.X,
                    stepPoint.Y + clearancePoint.Y,
                    stepPoint.Z + clearancePoint.Z
                    );
                if (unpassableByCLZPoints.Contains(currentPoint))
                    return false;
            }

            return true;
        }

    }
}
