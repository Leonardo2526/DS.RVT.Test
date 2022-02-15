﻿using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Lines;
using System.Collections.Generic;


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

            Dictionary<Outline, List<XYZ>> pointsInOutlines = GetPointsInOutlines();


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
                    else
                    {
                        //PassablePoints.Add(point);

                    }
                }
            }

            foreach (XYZ point in SpacePoints)
            {
                if (!UnpassablePoints.Contains(point))
                    PassablePoints.Add(point);  
            }

        }

        private Dictionary<Outline, List<XYZ>> GetPointsInOutlines()
        {
            Dictionary<Outline, List<XYZ>> pointsInOutlines = new Dictionary<Outline, List<XYZ>>();

            foreach (KeyValuePair<Outline, List<Solid>> keyValue in OutlinesWithSolids)
            {
                List<XYZ> points = new List<XYZ>();
                foreach (XYZ point in SpacePoints)
                {
                    //Is point inside outline
                    if (keyValue.Key.MinimumPoint.X <= point.X && point.X < keyValue.Key.MaximumPoint.X &&
                                     keyValue.Key.MinimumPoint.Y <= point.Y && point.Y < keyValue.Key.MaximumPoint.Y &&
                                     keyValue.Key.MinimumPoint.Z <= point.Z && point.Z < keyValue.Key.MaximumPoint.Z)
                        points.Add(point);
                }
                pointsInOutlines.Add(keyValue.Key, points);
            }
            return pointsInOutlines;

        }
    }
}
