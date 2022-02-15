using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Lines;
using System.Collections.Generic;


namespace DS.RVT.ModelSpaceFragmentation
{
    class PointsSeparator
    {
        List<XYZ> SpacePoints;
        List<Outline> Outlines;

        public PointsSeparator(List<XYZ> spacePoints, List<Outline> outlines)
        {
            SpacePoints = spacePoints;
            Outlines = outlines;
        }

        public List<XYZ> PassablePoints { get; set; } = new List<XYZ>();
        public List<XYZ> UnpassablePoints { get; set; } = new List<XYZ>();

        public void Separate()
        {
            LineCreator lineCreator = new LineCreator();
            SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();
            LineCollision lineCollision = new LineCollision(intersectOptions);

            PointInSolidChecker pointInSolidChecker = new PointInSolidChecker(lineCreator, lineCollision);


            foreach (XYZ point in SpacePoints)
            {
                if (!IsPointInOutline(point))
                {
                    PassablePoints.Add(point);
                    continue;
                }
                else
                {
                    UnpassablePoints.Add(point);

                }

                //if (pointInSolidChecker.IsPointInSolid(point))
                //{
                //    UnpassablePoints.Add(point);
                //}
                //else
                //{
                //    PassablePoints.Add(point);

                //}
            }
        }

        private bool IsPointInOutline(XYZ point)
        {
            foreach (Outline outline in Outlines)
            {
                if (outline.MinimumPoint.X <= point.X && point.X <= outline.MaximumPoint.X &&
                    outline.MinimumPoint.Y <= point.Y && point.Y <= outline.MaximumPoint.Y &&
                    outline.MinimumPoint.Z <= point.Z && point.Z <= outline.MaximumPoint.Z)
                    return true;
            }
            return false;
        }
    }
}
