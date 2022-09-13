using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.ConnectionPointService.PointModel
{
    public interface IConnectionPoint
    {
        public XYZ Point { get; }
    }
}
