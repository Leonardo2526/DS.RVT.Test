using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using OLMP.RevitAPI.Tools.Extensions;
using Rhino.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    public static class NewExtensions
    {
        public static IEnumerable<Face> ToList(this FaceArray faceArray)
        {
            var faces = new List<Face>();
            foreach (Face face in faceArray)
            { faces.Add(face); }
            return faces;
        }

        public static IEnumerable<T> AsEnumerable<T>(this IEnumerable resultArray)
        {
            var intersectionResults = new List<T>();
            foreach (T intersectionResult in resultArray)
            { intersectionResults.Add(intersectionResult); }
            return intersectionResults;
        }

        public static View3D TryGetView3D(this Document activeDoc)
            => new FilteredElementCollector(activeDoc)
            .OfClass(typeof(View3D))
            .Cast<View3D>()
            .Where(v => v.IsTemplate == false && v.IsPerspective == false)
            .FirstOrDefault();


        public static Floor FindNearestFloor(this XYZ point, Document activeDoc)
        {
            var view3D = TryGetView3D(activeDoc);
            var intersector = new ReferenceIntersector(
               new ElementCategoryFilter(BuiltInCategory.OST_Floors),
               FindReferenceTarget.All, view3D);
            var vector = XYZ.BasisZ.Negate();
            var rwC = intersector.FindNearest(point, vector);
            return rwC is null ?
                null :
                activeDoc.GetElement(rwC.GetReference().ElementId) as Floor; ;
        }

        public static Element FindNearestCeiling(this XYZ point, Document activeDoc)
        {
            var view3D = TryGetView3D(activeDoc);
            var intersector = new ReferenceIntersector(
               new ElementMulticategoryFilter(
                   new List<BuiltInCategory>()
                   { BuiltInCategory.OST_Floors, BuiltInCategory.OST_Ceilings}),
               FindReferenceTarget.All, view3D);

            var vector = XYZ.BasisZ;
            var rwC = intersector.FindNearest(point, vector);
            var element = rwC is null ? null : activeDoc.GetElement(rwC.GetReference().ElementId);

            return element is not null && (element is Floor || element is Ceiling) ?
                element :
                null;
        }

        public static Floor FindNearestFloor(this Room room, Document activeDoc)
        {
            var roomPoint = GetCenterPoint(room);
            return FindNearestFloor(roomPoint, activeDoc);
        }

        public static Element FindNearestCeiling(this Room room, Document activeDoc)
        {
            var roomPoint = GetCenterPoint(room);
            return FindNearestCeiling(roomPoint, activeDoc);
        }


        public static XYZ GetCenterPoint(this Room room)
        {
            var roomPoint = room.Location as LocationPoint;
            var elev = (room.UpperLimit.Elevation - room.Level.Elevation) / 2;

            return new XYZ(roomPoint.Point.X, roomPoint.Point.Y, roomPoint.Point.Z + elev);
        }

        /// <summary>
        /// Get the height of the <paramref name="floor"/>.
        /// </summary>
        /// <param name="floor"></param>
        /// <returns>
        /// <paramref name="floor"/>'s thickness.
        /// </returns>
        public static double GetThickness(this Floor floor) =>
            floor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();

        /// <summary>
        /// Get the height of the <paramref name="ceiling"/>.
        /// </summary>
        /// <param name="ceiling"></param>
        /// <returns>
        /// <paramref name="ceiling"/>'s thickness.
        /// </returns>
        public static double GetThickness(this Ceiling ceiling)
        {
            var hasThickness = ceiling.get_Parameter(BuiltInParameter.CEILING_HAS_THICKNESS_PARAM);
            //var hasThickness = ceiling.get_Parameter(BuiltInParameter.CEILING_HAS_THICKNESS_PARAM).HasValue;
            return hasThickness == default ?
                0 :
                ceiling.get_Parameter(BuiltInParameter.CEILING_THICKNESS_PARAM).AsDouble();
        }


        public static CurveLoop GetBottomProfile(this Wall wall)
        {
            var curve = wall.GetLocationCurve();

            var offset = wall.Width / 2;
            var refVector = XYZ.BasisZ;

            var offsetCurve1 = curve.CreateOffset(offset, -refVector);
            var offsetCurve2 = curve.CreateOffset(offset, refVector).CreateReversed();

            var p1 = offsetCurve1.GetEndPoint(0);
            var p2 = offsetCurve1.GetEndPoint(1);
            var p3 = offsetCurve2.GetEndPoint(0);
            var p4 = offsetCurve2.GetEndPoint(1);

            var line1 = Line.CreateBound(p2, p3);
            var line2 = Line.CreateBound(p4, p1);
            return CurveLoop.Create(new List<Curve>() 
            { offsetCurve1, line1, offsetCurve2, line2 });
        }

        public static Solid GetFullSolid(this Wall wall)
        {
            var profile = GetBottomProfile(wall);
            return GeometryCreationUtilities
               .CreateExtrusionGeometry(
               new List<CurveLoop> { profile },
               XYZ.BasisZ, wall.GetHeigth());
        }
    }
}
