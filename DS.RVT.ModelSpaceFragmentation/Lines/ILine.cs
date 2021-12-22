using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RVT.ModelSpaceFragmentation.Points;
using Autodesk.Revit.DB;

namespace DS.RVT.ModelSpaceFragmentation
{
    interface ILine
    {
        Line Create();
    }
}
