using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.WaveAlgorythm
{
    class Collision
    {
        readonly Application App;
        readonly UIApplication Uiapp;
        readonly Document Doc;
        readonly UIDocument Uidoc;

        public Collision(Application app, UIApplication uiapp, Document doc, UIDocument uidoc)
        {
            App = app;
            Uiapp = uiapp;
            Doc = doc;
            Uidoc = uidoc;
        }


        public XYZ FindCollision(FamilyInstance instance, ExclusionFilter exclusionFilter)
        {
            ElementIntersectsElementFilter elementIntersectsElementFilter = 
                new ElementIntersectsElementFilter(instance);

            //Get collector with filtered elements
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.WherePasses(exclusionFilter);
            collector.WherePasses(elementIntersectsElementFilter);
           

            XYZ point = null;
            if (collector.Count()> 0)
            {
                LocationPoint locationPopint = instance.Location as LocationPoint;
                point = new XYZ(locationPopint.Point.X, locationPopint.Point.Y, locationPopint.Point.Z);
            }

            return point;
        }

       


        private Solid GetSolid(FamilyInstance instance)
        {
            GeometryElement geomElement = instance.get_Geometry(new Options());

            Solid solid = null;
            foreach (GeometryObject geomObj in geomElement)
            {
                solid = geomObj as Solid;
                if (solid != null) break;
            }

            return solid;
        }
    }
}
