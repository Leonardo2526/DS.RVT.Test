using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    public class EdgeElement
    {
        public Element Element { get; }
        public XYZ Point { get; }

        public EdgeElement(Element element, XYZ point)
        {
            Element = element;
            Point = point;
        }
    }
}
