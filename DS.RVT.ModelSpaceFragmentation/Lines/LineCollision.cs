using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    class LineCollision
    {
        readonly Document Doc;

        public LineCollision(Document doc)
        {
            Doc = doc;
        }

        public List<CurveExtents> CurvesExtIntersection { get; set; } = new List<CurveExtents>();

        public IList<Element> GetElementsCurveCollisions(Curve curve, Dictionary<Element, List<Solid>> elementsSolids)
        {
            IList<Element> intersectedElements = new List<Element>();

            foreach (KeyValuePair<Element, List<Solid>> keyValue in elementsSolids)
            {
                foreach (Solid solid in keyValue.Value)
                {
                    SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();

                    //Get intersections with curve
                    SolidCurveIntersection intersection = solid.IntersectWithCurve(curve, intersectOptions);
                    if (intersection.SegmentCount != 0)
                    {
                        TransactionCreator transactionCreator = new TransactionCreator(Doc);
                        CurvesExtIntersection.Add(intersection.GetCurveSegmentExtents(0));

                        intersectedElements.Add(keyValue.Key);
                    }
                }
            }

            return intersectedElements;
        }

       

    }
}
