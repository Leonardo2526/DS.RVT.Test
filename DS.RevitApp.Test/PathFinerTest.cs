using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using DS.RevitLib.Utils.ModelCurveUtils;
using PathFinderLib;

namespace DS.RevitApp.Test
{
    internal class PathFinerTest
    {
        private readonly Document _doc;

        public PathFinerTest(Document doc)
        {
            _doc = doc;
        }
        public async void Run()
        {
            XYZ p1 = new XYZ(0,0, 0);
            XYZ p2 = new XYZ(10,0, 0);


            //var finder = new PathFinderToPointsList(_doc, p1, new List<XYZ> { p2 },
            //   NeighboursOptions._2D, 1000.mmToFyt2(), 1000.mmToFyt2());
            //var points = await finder.FindPath(new CancellationTokenSource().Token, 50);

            //ShowPath(points);
        }
        private void ShowPath(List<XYZ> path)
        {
            var mcreator = new ModelCurveCreator(_doc);
            for (int i = 0; i < path.Count - 1; i++)
            {
                mcreator.Create(path[i], path[i + 1]);
                var line = Line.CreateBound(path[i], path[i + 1]);
            }
        }
    }
}
