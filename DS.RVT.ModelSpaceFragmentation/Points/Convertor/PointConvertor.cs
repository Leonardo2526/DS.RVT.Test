using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Path;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Points
{
    class PointConvertor
    {
        public static XYZ StepPointToXYZ(StepPoint stepPoint)
        {
            XYZ refPoint = new XYZ(stepPoint.X * ModelSpacePointsGenerator.PointsStepF,
            stepPoint.Y * ModelSpacePointsGenerator.PointsStepF,
            stepPoint.Z * ModelSpacePointsGenerator.PointsStepF);

            return new XYZ(PointsInfo.MinBoundPoint.X + refPoint.X, 
                PointsInfo.MinBoundPoint.Y + refPoint.Y,
                PointsInfo.MinBoundPoint.Z + refPoint.Z);

        }

        public static StepPoint XYZToStepPoint(XYZ point)
        {
            XYZ refPoint = new XYZ(point.X - PointsInfo.MinBoundPoint.X,
                point.Y - PointsInfo.MinBoundPoint.Y,
                point.Z - PointsInfo.MinBoundPoint.Z);

            return new StepPoint((int)Math.Round(refPoint.X / ModelSpacePointsGenerator.PointsStepF),
            (int)Math.Round(refPoint.Y / ModelSpacePointsGenerator.PointsStepF),
            (int)Math.Round(refPoint.Z / ModelSpacePointsGenerator.PointsStepF));

        }

    }
}
