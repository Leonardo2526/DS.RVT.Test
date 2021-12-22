using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PointsSeparator
    {
        List<XYZ> SpacePoints;

        public PointsSeparator(List<XYZ> spacePoints)
        {
            SpacePoints = spacePoints;
        }

        public List<XYZ> GetPassablePoints()
        {
            List<XYZ> passablePoints = new List<XYZ>();

            foreach (XYZ point in SpacePoints)
            {
                if (!PointInSolidChecker.IsPointInSolid(point))
                {
                    passablePoints.Add(point);
                }
            }

                return passablePoints;
        }

        public List<XYZ> GetUnpassablePoints()
        {
            List<XYZ> unpassablePoints = new List<XYZ>();

            return unpassablePoints;
        }
    }
}
