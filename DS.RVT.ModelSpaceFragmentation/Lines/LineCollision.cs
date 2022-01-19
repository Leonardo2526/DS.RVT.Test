using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    class LineCollision
    {
        SolidCurveIntersectionOptions intersectOptions;

        public LineCollision(SolidCurveIntersectionOptions intersectOptions)
        {
            this.intersectOptions = intersectOptions;
        }

        public List<CurveExtents> GetElementsCurveCollisions(Curve curve, Dictionary<Element, List<Solid>> elementsSolids)
        {
            List<CurveExtents> CurvesExtIntersection = new List<CurveExtents>();

            foreach (KeyValuePair<Element, List<Solid>> keyValue in elementsSolids)
            {
                foreach (Solid solid in keyValue.Value)
                {

                    //Get intersections with curve
                    SolidCurveIntersection intersection = solid.IntersectWithCurve(curve, intersectOptions);                  
                    if (intersection.SegmentCount != 0)
                        CurvesExtIntersection.Add(intersection.GetCurveSegmentExtents(0));
                }
            }
            return CurvesExtIntersection;
        }

        public bool NewGetElementsCurveCollisions(Curve curve, Dictionary<Element, List<Solid>> elementsSolids)
        {
            List<CurveExtents> CurvesExtIntersection = new List<CurveExtents>();

            foreach (KeyValuePair<Element, List<Solid>> keyValue in elementsSolids)
            {
                foreach (Solid solid in keyValue.Value)
                {
                    //Get intersections with curve
                    SolidCurveIntersection intersection = solid.IntersectWithCurve(curve, intersectOptions);
                    if (intersection.SegmentCount != 0)
                    {
                        CurveExtents curveExt = intersection.GetCurveSegmentExtents(0);
                        if (curveExt.StartParameter == 0)
                            return true;
                    }                   
                }
            }
            return false;
        }
    }
}
