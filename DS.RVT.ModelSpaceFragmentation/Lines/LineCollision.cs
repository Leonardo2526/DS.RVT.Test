using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    class LineCollision
    {   
        public List<CurveExtents> GetElementsCurveCollisions(Curve curve, Dictionary<Element, List<Solid>> elementsSolids)
        {
            List<CurveExtents> CurvesExtIntersection = new List<CurveExtents>();

            foreach (KeyValuePair<Element, List<Solid>> keyValue in elementsSolids)
            {
                foreach (Solid solid in keyValue.Value)
                {
                    SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();

                    //Get intersections with curve
                    SolidCurveIntersection intersection = solid.IntersectWithCurve(curve, intersectOptions);                  
                    if (intersection.SegmentCount != 0)
                        CurvesExtIntersection.Add(intersection.GetCurveSegmentExtents(0));
                }
            }
            return CurvesExtIntersection;
        }
    }
}
