using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
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


     
    }
}
