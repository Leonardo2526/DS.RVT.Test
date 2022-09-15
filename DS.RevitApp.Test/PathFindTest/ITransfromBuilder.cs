using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.PathFindTest
{
    public interface ITransfromBuilder
    {
        public Dictionary<FamilyInstance, Transform> Build();
    }
}
