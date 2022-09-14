using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.PathFinders
{
    public interface IPathFinder
    {
        public List<XYZ> FindPath(XYZ point1, XYZ point2);
    }
}
