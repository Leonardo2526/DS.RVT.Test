using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.SwitchSolution.Models
{
    internal class Points
    {
        public Points()
        {
            PointsLists = GetAllPoints();
        }

        public List<List<XYZ>> PointsLists { get; set; } = new List<List<XYZ>>();

        private List<List<XYZ>> GetAllPoints()
        {
            int offset = 2;

            for (int i = 0; i < 5; i++)
            {
                PointsLists.Add(GetPoints(offset * i));
            }

            return PointsLists;
        }

        private List<XYZ> GetPoints(int offset)
        {
            return new List<XYZ>
            {
                new XYZ(0, offset, 0),
                new XYZ(5, offset, 0)
            };
        }
    }
}
