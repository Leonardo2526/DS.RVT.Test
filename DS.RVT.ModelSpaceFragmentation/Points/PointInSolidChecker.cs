using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Lines;

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

        public bool IsPointInSolid(XYZ point)
        {
            RayCreator ray = new RayCreator(point);
            Line rayLine = lineCreator.Create(ray);

            bool IfOneLineIntersections = lineCollision.GetElementsCurveCollisions(rayLine, ModelSolid.SolidsInModel);
            if (IfOneLineIntersections)
                return true;

            return false;
        }

    }
}
