using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCmd.SpaceBoundary
{
    public record BoundaryCurve(ElementId ElementId, Curve Curve);

}
