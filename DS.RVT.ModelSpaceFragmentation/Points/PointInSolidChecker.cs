using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Lines;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PointInSolidChecker
    {
        public static bool IsPointInSolid(Document Doc, XYZ point)
        {
            LineCreator lineCreator = new LineCreator();
            Ray ray = new Ray(point);
            Line rayLine = lineCreator.Create(ray);

            //TransactionCreator transaction = new TransactionCreator(Doc);
            //transaction.Create(new ModelCurveTransaction(ray.StartPoint, ray.EndPoint));

            List<Line> lines = new List<Line>()
            {
                rayLine
            };

            LineCollision lineCollision = new LineCollision(Doc);

            lineCollision.GetAllModelSolids(lines);

            foreach (Line gLine in lines)
            {
                IList<Element> CheckCollisions = lineCollision.GetAllLinesCollisions(gLine);

                if (CheckCollisions.Count != 0)
                    return false;
            }

            return true;
        }

    }
}
