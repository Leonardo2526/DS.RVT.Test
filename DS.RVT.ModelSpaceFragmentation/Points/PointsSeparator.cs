using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RVT.ModelSpaceFragmentation.Lines;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            Dictionary<Outline, List<XYZ>> sortedPoints = SortPointsByOutlinesTPL();


            //Separate points inside each outline 
            LineCreator lineCreator = new LineCreator();
            SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();
            LineCollision lineCollision = new LineCollision(intersectOptions);
            PointInSolidChecker pointInSolidChecker = new PointInSolidChecker(lineCreator, lineCollision);

            foreach (KeyValuePair<Outline, List<XYZ>> keyValue in sortedPoints)
            {
                OutlinesWithSolids.TryGetValue(keyValue.Key, out List<Solid> solids);

                List<XYZ> pointsForSearch = GetPointsInSolids(keyValue.Value, solids);

                foreach (XYZ point in pointsForSearch)
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

        private Dictionary<Outline, List<XYZ>> SortPointsByOutlines2()
        {
            Dictionary<Outline, List<XYZ>> sortedPoints = new Dictionary<Outline, List<XYZ>>();

            List<XYZ> pointsInAllOutlines = GetPointsInOutlines();

            foreach (KeyValuePair<Outline, List<Solid>> keyValue in OutlinesWithSolids)
            {
                List<XYZ> pointsInOutline = new List<XYZ>();

                List<int> indexes = new List<int>();
                for (int i = 0; i < pointsInAllOutlines.Count; i++)
                {
                    if (keyValue.Key.Contains(pointsInAllOutlines[i], 0))
                    {
                        indexes.Add(i);
                        pointsInOutline.Add(pointsInAllOutlines[i]);
                    }
                }

                //for (int i = indexes.Count - 1; i-- > 0;)
                //    pointsInAllOutlines.RemoveAt(i);

                sortedPoints.Add(keyValue.Key, pointsInOutline);
            }
            return sortedPoints;
        }

        private Dictionary<Outline, List<XYZ>> SortPointsByOutlines1()
        {
            Dictionary<Outline, List<XYZ>> sortedPoints = new Dictionary<Outline, List<XYZ>>();

            List<XYZ> pointsInOutlines = GetPointsInOutlines();

            foreach (XYZ point in pointsInOutlines)
            {
                foreach (KeyValuePair<Outline, List<Solid>> keyValue in OutlinesWithSolids)
                {
                    if (sortedPoints.Values.Count >= SpaceZone.ZonePointsCount)
                        continue;

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

        private Dictionary<Outline, List<XYZ>> SortPointsByOutlines()
        {
            Dictionary<Outline, List<XYZ>> sortedPoints = new Dictionary<Outline, List<XYZ>>();

            List<XYZ> pointsInOutlines = GetPointsInOutlines();

            foreach (KeyValuePair<Outline, List<Solid>> keyValue in OutlinesWithSolids)
            {
                sortedPoints.Add(keyValue.Key, new List<XYZ>());
            }

            foreach (XYZ point in pointsInOutlines)
            {
                foreach (KeyValuePair<Outline, List<Solid>> keyValue in OutlinesWithSolids)
                {

                    //Is point inside outline
                    if (keyValue.Key.Contains(point, 0))
                    {
                        sortedPoints.TryGetValue(keyValue.Key, out List<XYZ> points);

                        points.Add(point);
                        sortedPoints[keyValue.Key] = points;
                        break;
                    }


                }
            }


            return sortedPoints;

        }

        private Dictionary<Outline, List<XYZ>> SortPointsByOutlinesTPL()
        {

            List<XYZ> pointsInOutlines = GetPointsInOutlines();

            ParallelSort2 parallelSort = new ParallelSort2(OutlinesWithSolids, pointsInOutlines);
            parallelSort.RunSort();
            Dictionary<Outline, List<XYZ>> sortedPoints = parallelSort.SortedPoints;

            //ParallelSort parallelSort = new ParallelSort(OutlinesWithSolids, pointsInOutlines);
            //parallelSort.RunSort();
            //Dictionary<Outline, List<XYZ>> sortedPoints = parallelSort.SortedPoints;

            //Multithreadsort multithreadsort = new Multithreadsort(OutlinesWithSolids, pointsInOutlines);
            //multithreadsort.RunSort();
            //Dictionary<Outline, List<XYZ>> sortedPoints = multithreadsort.SortedPoints;

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
                if (outline.Contains(SpacePoints[i], 0))
                    xYZs.Add(SpacePoints[i]);
            }

            return xYZs;
        }

        private List<XYZ> GetPointsInSolids(List<XYZ> points , List<Solid> solids)
        {
            List<XYZ> pointsForSearch = new List<XYZ>();

            foreach (Solid solid in solids)
            {
                Transform transform = solid.GetBoundingBox().Transform;
                Solid solidTransformed = SolidUtils.CreateTransformed(solid, transform);

                XYZ minPoint = solidTransformed.GetBoundingBox().Min;
                XYZ maxPoint = solidTransformed.GetBoundingBox().Max;

                XYZ minTrPoint = transform.OfPoint(minPoint);
                XYZ maxTrPoint = transform.OfPoint(maxPoint);

                Outline outline = new Outline(minTrPoint, maxTrPoint);

                foreach (XYZ point in points)
                {
                    //if (point == null)
                    //{
                    //    TaskDialog.Show("Revit", "null Point");
                    //}
                    if (point != null && outline.Contains(point, 0))
                        pointsForSearch.Add(point);
                }
            }

            return pointsForSearch;
        }

    }
}
