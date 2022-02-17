using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation
{
    class Multithreadsort
    {

        private readonly Dictionary<Outline, List<Solid>> OutlinesWithSolids;
        private readonly List<XYZ> PointsInOutlines;

        public Dictionary<Outline, List<XYZ>> SortedPoints = new Dictionary<Outline, List<XYZ>>();

        public Multithreadsort(Dictionary<Outline, List<Solid>> outlinesWithSolids, List<XYZ> pointsInOutlines)
        {
            OutlinesWithSolids = outlinesWithSolids;
            PointsInOutlines = pointsInOutlines;
        }


        private void Fill()
        {
            foreach (KeyValuePair<Outline, List<Solid>> keyValue in OutlinesWithSolids)
            {
                SortedPoints.Add(keyValue.Key, new List<XYZ>());
            }
        }

        private void CheckPoints(int i)
        {
            foreach (KeyValuePair<Outline, List<Solid>> keyValue in OutlinesWithSolids)
            {

                //Is point inside outline
                if (keyValue.Key.Contains(PointsInOutlines[i], 0))
                {
                    SortedPoints.TryGetValue(keyValue.Key, out List<XYZ> points);

                    points.Add(PointsInOutlines[i]);
                    SortedPoints[keyValue.Key] = points;
                    break;
                }


            }
        }

        public void RunSort()
        {
            Fill();

            Task[] tasks = new Task[PointsInOutlines.Count];
            for (var i = 0; i < tasks.Length - 1; i++)
            {
                tasks[i] = new Task(() =>
                {
                    CheckPoints(i);
                });
                tasks[i].Start();   // запускаем задачу
            }
            Console.WriteLine("Завершение метода Main");

            Task.WaitAll(tasks); // ожидаем завершения всех задач
        }
    }
}
