using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation.WaveAlgorythm
{
    class InputData
    {
        public XYZ StartPoint { get; set; }
        public XYZ EndPoint { get; set; }
        public List<XYZ> UnpassablePoints { get; set; }

        public int W { get; set; } = ModelSpacePointsGenerator.Xcount;
        public int H { get; set; }= ModelSpacePointsGenerator.Ycount;
       
        public XYZ ZonePoint1 { get; set; } = BoundPoints.MinPoint;
        public XYZ ZonePoint2 { get; set; } = BoundPoints.MaxPoint;

        public static double PointsStepF { get; set; } = ModelSpacePointsGenerator.PointsStepF;

        public InputData (XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            UnpassablePoints = unpassablePoints;
        }

    }
}
