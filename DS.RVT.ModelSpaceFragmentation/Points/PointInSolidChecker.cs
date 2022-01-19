using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Lines;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PointInSolidChecker
    {
        readonly LineCreator lineCreator;

        public PointInSolidChecker(LineCreator lineCreator)
        {
            this.lineCreator = lineCreator;
        }

        public List<int> Count { get; set; } = new List<int>();

        public bool IsPointInSolid(XYZ point)
        {
            RayCreator ray = new RayCreator(point);
            Line rayLine = lineCreator.Create(ray);         

            LineCollision lineCollision = new LineCollision();

            List<CurveExtents> CurvesExtIntersection = lineCollision.GetElementsCurveCollisions(rayLine, ModelSolid.SolidsInModel);

            foreach (CurveExtents curveExt in CurvesExtIntersection)
            {
                if (curveExt.StartParameter == 0)
                    return true;
            }

            return false;
        }

    }
}
