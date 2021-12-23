using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation
{
    class ModelSpacePointsGenerator
    {
        XYZ Point1;
        XYZ Point2;
        readonly int PointsStep = 200;
        double pointsStepF;

        int Xcount;
        int Ycount;
        int Zcount;

        public ModelSpacePointsGenerator (XYZ p1, XYZ p2)
        {
            Point1 = p1;
            Point2 = p2;
        }

        public List<XYZ> Generate()
        {
            GetStepInFeets();
            GetCount();

            List<XYZ> spacePoints = new List<XYZ>();

            int z;
            for (z = 0; z < Zcount; z++)
            {
                double zStep = z * pointsStepF;

                int y;
                for (y = 0; y < Ycount; y++)
                {
                    double yStep = y * pointsStepF;

                    int x;
                    for (x = 0; x < Xcount; x++)
                    {
                        double xStep = x * pointsStepF;
                        XYZ point = new XYZ(Point1.X + xStep, Point1.Y + yStep, Point1.Z + zStep);
                        spacePoints.Add(point);
                    }
                }
            }            
           

            return spacePoints;
        }

        void GetStepInFeets()
        {
            pointsStepF = UnitUtils.Convert((double)PointsStep / 1000,
                                 DisplayUnitType.DUT_METERS,
                                 DisplayUnitType.DUT_DECIMAL_FEET);
        }

        void GetCount()
        {
            Xcount = (int)Math.Round((Point2.X - Point1.X) / pointsStepF);
            Ycount = (int)Math.Round((Point2.Y - Point1.Y) / pointsStepF);
            Zcount = (int)Math.Round((Point2.Z - Point1.Z) / pointsStepF);
        }


    }
}
