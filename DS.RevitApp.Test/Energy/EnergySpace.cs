using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.Energy
{
    public class EnergySpace
    {
        public EnergySpace(Space space)
        {
            Space = space;
        }


        public Space Space { get; set; }

        public void Show(Document activeDoc)
        {
            var options = new SpatialElementBoundaryOptions();
            var calculator = new SpatialElementGeometryCalculator(activeDoc, options);
            var result = calculator.CalculateSpatialElementGeometry(Space);
            var spaceSolid = result.GetGeometry();
            spaceSolid.ShowShape(activeDoc);
        }
       
    }
}
