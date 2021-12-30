using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class PointsCheker
    {      
        private bool IsEven(int a)
        {
            return (a % 2) == 0;
        }

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

        public bool IsStartEndPointAvailable(StepPoint point, List<StepPoint> clearancePoints)
        {
            bool checkUnpassablePoint = IsPointPassable(point);
            bool checkClearancePoint = IsClearanceZoneAvailable(point, clearancePoints);
            if (!checkUnpassablePoint | !checkClearancePoint)
            {
                TaskDialog.Show("Error", "Start or end point is unpassible!");
                return false;
            }

            return true;
        }

        public bool IsClearanceZoneAvailableOld(StepPoint stepPoint, 
            List<StepPoint> clearancePoints)
        {
            if (InputData.UnpassStepPoints.Count == 0)
                return true;

            foreach (StepPoint clearancePoint in clearancePoints)
            {
                StepPoint currentPoint = new StepPoint(
                    stepPoint.X + clearancePoint.X,
                    stepPoint.Y + clearancePoint.Y,
                    stepPoint.Z + clearancePoint.Z
                    );
                if (InputData.UnpassStepPoints.Contains(currentPoint))
                        return false;
            }

            return true;
        }

        public bool IsClearanceZoneAvailable(StepPoint stepPoint,
         List<StepPoint> clearancePoints)
        {
            if (InputData.UnpassStepPoints.Count == 0)
                return true;

            foreach (StepPoint clearancePoint in clearancePoints)
            {
                StepPoint currentPoint = new StepPoint(
                    stepPoint.X + clearancePoint.X,
                    stepPoint.Y + clearancePoint.Y,
                    stepPoint.Z + clearancePoint.Z
                    );
                if (InputData.UnpassStepPoints.Contains(currentPoint))
                    return false;
            }

            return true;
        }

    }
}
