using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Lines;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PointsSeparator
    {
        List<XYZ> SpacePoints;
        Dictionary<Outline, List<Solid>> OutlinesWithSolids;

        public PointsSeparator(List<XYZ> spacePoints, Dictionary<Outline, List<Solid>> outlinesSolids)
        {
            SpacePoints = spacePoints;
            OutlinesWithSolids = outlinesSolids;
        }

        public List<XYZ> PassablePoints { get; set; } = new List<XYZ>();
        public List<XYZ> UnpassablePoints { get; set; } = new List<XYZ>();

        public void Separate()
        {

            Dictionary<Outline, List<XYZ>> pointsInOutlines = SortPointsByOutlines();


            //Separate points inside each outline
            LineCreator lineCreator = new LineCreator();
            SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();
            LineCollision lineCollision = new LineCollision(intersectOptions);
            PointInSolidChecker pointInSolidChecker = new PointInSolidChecker(lineCreator, lineCollision);

            foreach (KeyValuePair<Outline, List<XYZ>> keyValue in pointsInOutlines)
            {
                OutlinesWithSolids.TryGetValue(keyValue.Key, out List<Solid> solids);

                foreach (XYZ point in keyValue.Value)
                {
                    if (pointInSolidChecker.IsPointInSolid(point, solids))
                        UnpassablePoints.Add(point);
                }
            }

            //For visualization only
            //foreach (XYZ point in SpacePoints)
            //{
            //    if (!UnpassablePoints.Contains(point))
            //        PassablePoints.Add(point);
            //}

        }

        private Dictionary<Outline, List<XYZ>> SortPointsByOutlines()
        {
            Dictionary<Outline, List<XYZ>> sortedPoints = new Dictionary<Outline, List<XYZ>>();

            List<XYZ> pointsInOutlines = GetPointsInOutlines();

            foreach (XYZ point in pointsInOutlines)
            {
                foreach (KeyValuePair<Outline, List<Solid>> keyValue in OutlinesWithSolids)
                {

                    //Is point inside outline
                    if (keyValue.Key.Contains(point, 0))
                    {
                        if (!sortedPoints.ContainsKey(keyValue.Key))
                            sortedPoints.Add(keyValue.Key, new List<XYZ>());

                        sortedPoints.TryGetValue(keyValue.Key, out List<XYZ> points);

                        points.Add(point);
                        sortedPoints[keyValue.Key] = points;
                        break;
                    }
                   
                }
            }
             
            
            return sortedPoints;

        }


        private List<XYZ> GetPointsInOutlines()
        {
            List<XYZ> xYZs = new List<XYZ>();

            XYZ minPoint = OutlinesWithSolids.First().Key.MinimumPoint;
            XYZ maxPoint = OutlinesWithSolids.Last().Key.MaximumPoint;

            Outline outline = new Outline(minPoint, maxPoint);

            for (int i = 0; i < SpacePoints.Count; i++)
            {
                if (outline.Contains(SpacePoints[i],0))
                    xYZs.Add(SpacePoints[i]);
            }

            return xYZs;
        }
    }
}
