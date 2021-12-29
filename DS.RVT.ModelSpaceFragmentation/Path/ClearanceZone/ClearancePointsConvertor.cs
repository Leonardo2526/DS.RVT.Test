using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Path;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Points
{
    class ClearancePointsConvertor
    {
        public List<XYZ> GetClearancePointsByXYZ(XYZ basePoint, List<StepPoint> ClerancePoints)
        {
            List<XYZ> modelSpacePoints = new List<XYZ>();

            foreach (StepPoint stepPoint in ClerancePoints)
            {
                XYZ point = new XYZ(basePoint.X + stepPoint.X * InputData.PointsStepF,
                    basePoint.Y + stepPoint.Y * InputData.PointsStepF,
                    basePoint.Z + stepPoint.Z * InputData.PointsStepF);
                modelSpacePoints.Add(point);
            }

            return modelSpacePoints;
        }
    }
}
