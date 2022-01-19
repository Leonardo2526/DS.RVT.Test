using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Lines;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PointInSolidChecker
    {
        readonly LineCreator lineCreator;
        readonly LineCollision lineCollision;

        public PointInSolidChecker(LineCreator lineCreator, LineCollision lineCollision)
        {
            this.lineCreator = lineCreator;
            this.lineCollision = lineCollision;

        }

        public List<int> Count { get; set; } = new List<int>();

        public bool IsPointInSolid(XYZ point)
        {
            RayCreator ray = new RayCreator(point);
            Line rayLine = lineCreator.Create(ray);       


            bool checker = lineCollision.NewGetElementsCurveCollisions(rayLine, ModelSolid.SolidsInModel);
            if (checker)
                return true;
            //List<CurveExtents> CurvesExtIntersection = lineCollision.GetElementsCurveCollisions(rayLine, ModelSolid.SolidsInModel);

            //foreach (CurveExtents curveExt in CurvesExtIntersection)
            //{
            //    if (curveExt.StartParameter == 0)
            //        return true;
            //}

            return false;
        }

    }
}
