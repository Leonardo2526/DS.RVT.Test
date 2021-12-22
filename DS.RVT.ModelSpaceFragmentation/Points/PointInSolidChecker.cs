using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RVT.ModelSpaceFragmentation.Lines;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PointInSolidChecker
    {
        public static bool IsPointInSolid(XYZ point)
        {
            LineCreator lineCreator = new LineCreator();
            Line ray = lineCreator.Create(new Ray(point));


            return true;
        }
    }
}
