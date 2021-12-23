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

        public List<XYZ> PassablePoints { get; set; } = new List<XYZ>();
        public List<XYZ> UnpassablePoints { get; set; } = new List<XYZ>();


        public void Separate(Document Doc)
        {
            foreach (XYZ point in SpacePoints)
            {
                if (!PointInSolidChecker.IsPointInSolid(Doc, point))
                {
                    UnpassablePoints.Add(point);
                }
                else
                {
                    PassablePoints.Add(point);

                }
            }
        }
    }
}
