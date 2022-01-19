using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Lines;
using System.Collections.Generic;


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

        public void Separate()
        {
            LineCreator lineCreator = new LineCreator();
            PointInSolidChecker pointInSolidChecker = new PointInSolidChecker(lineCreator);

            foreach (XYZ point in SpacePoints)
            {
                if (pointInSolidChecker.IsPointInSolid(point))
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
