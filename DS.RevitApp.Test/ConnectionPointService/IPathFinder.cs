using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.ConnectionPointService
{
    public interface IPathFinder
    {
        public List<XYZ> FindPath();
    }
}
