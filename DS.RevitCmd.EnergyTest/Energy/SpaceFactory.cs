using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCmd.EnergyTest
{
    public class SpaceFactory : ISpaceFactory
    {
        private static Plane _basePlane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
        private readonly Document _doc;

        public SpaceFactory(Document doc)
        {
            _doc = doc;
        }

        public Space Create(Room room)
        {
            var lp = room.Location as LocationPoint;
            var roomLocationPoint = lp.Point;

            _basePlane.Project(roomLocationPoint, out var uv, out var d1);
            var space = _doc.Create.NewSpace(room.Level, uv);
            if (room.Level.Id != room.UpperLimit.Id)
            {
                space.UpperLimit = room.UpperLimit;
                space.LimitOffset = room.LimitOffset;
            }

            return space;
        }
    }
}
