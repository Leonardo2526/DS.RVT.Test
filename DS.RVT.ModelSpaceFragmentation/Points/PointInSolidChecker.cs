using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Lines;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PointInSolidChecker
    {
        public List<int> Count { get; set; } = new List<int>();

        public bool IsPointInSolid(Document Doc, XYZ point)
        {
            LineCreator lineCreator = new LineCreator();
            Ray ray = new Ray(point);
            Line rayLine = lineCreator.Create(ray);         

            LineCollision lineCollision = new LineCollision(Doc);

            IList<Element> CheckCollisions = lineCollision.GetElementsCurveCollisions(rayLine, ModelSolid.SolidsInModel);

            foreach (CurveExtents curveExt in lineCollision.CurvesExtIntersection)
            {                
                if (curveExt.StartParameter == 0 || curveExt.EndParameter == 0)
                    return true;
            }
          
            return false;
        }

    }
}
