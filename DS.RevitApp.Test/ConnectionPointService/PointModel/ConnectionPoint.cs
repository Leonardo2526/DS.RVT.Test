using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.ConnectionPointService.PointModel
{
    internal class ConnectionPoint : IConnectionPoint
    {
        public ConnectionPoint(XYZ point, Element element)
        {
            Point = point;
            Element = element;
        }
        public XYZ Point { get; private set; }
        public Element Element { get; private set; }
    }
}
