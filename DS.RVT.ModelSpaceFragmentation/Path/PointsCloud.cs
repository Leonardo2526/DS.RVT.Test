using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class PointsCloud
    {
        public static List<XYZ> PointsCloudList { get; set; }

        public static List<XYZ> GetByCenterPoint(XYZ centerPoint, double distance)
        {
            foreach (XYZ unpassablePoint in SpaceFragmentator.UnpassablePoints)
            {
                if (centerPoint.DistanceTo(unpassablePoint) <= distance)
                    PointsCloudList.Add(unpassablePoint);
            }

            return PointsCloudList;
        }
    }
}
