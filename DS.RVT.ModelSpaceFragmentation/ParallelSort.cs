using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation
{
    class ParallelSort
    {
        private readonly Dictionary<Outline, List<Solid>> OutlinesWithSolids;
        private readonly List<XYZ> PointsInOutlines;

        public Dictionary<Outline, List<XYZ>> SortedPoints  = new Dictionary<Outline, List<XYZ>>();

        public ParallelSort(Dictionary<Outline, List<Solid>> outlinesWithSolids, List<XYZ> pointsInOutlines)
        {
            OutlinesWithSolids = outlinesWithSolids;
            PointsInOutlines = pointsInOutlines;
        }

        private void CheckPoints(int n)
        {
            foreach (KeyValuePair<Outline, List<Solid>> keyValue in OutlinesWithSolids)
            {

                //Is point inside outline
                if (keyValue.Key.Contains(PointsInOutlines[n], 0))
                {
                    SortedPoints.TryGetValue(keyValue.Key, out List<XYZ> points);

                    points.Add(PointsInOutlines[n]);
                    SortedPoints[keyValue.Key] = points;
                    break;
                }


            }
        }


        public void RunSort()
        {
            Fill();

            Parallel.For(1, PointsInOutlines.Count, CheckPoints);
        }

        private void Fill()
        {
            foreach (KeyValuePair<Outline, List<Solid>> keyValue in OutlinesWithSolids)
            {
                SortedPoints.Add(keyValue.Key, new List<XYZ>());
            }
        }
    }
}
