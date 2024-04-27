using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLib
{
    public class RequestTask
    {
        public RequestTask(Line line, XYZ p1, XYZ p2)
        {
            Line = line;
            P1 = p1;
            P2 = p2;
        }

        public Line Line { get; }
        public XYZ P1 { get; }
        public XYZ P2 { get; }
    }

    public class Response
    {
        public Response(List<XYZ> points)
        {
            Points = points;
        }

        public List<XYZ> Points { get; }
    }
}
