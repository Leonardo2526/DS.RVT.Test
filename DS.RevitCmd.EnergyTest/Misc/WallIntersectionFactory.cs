using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitApp.Test;
using MoreLinq;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Graphs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
namespace DS.RevitCmd.EnergyTest.SpaceBoundary
{
    internal class WallIntersectionFactory
    {

        private readonly Document _activeDoc;
        private readonly SolidElementIntersectionFactoryBase<Element> _intersectionFactory;

        public WallIntersectionFactory(
            Document activeDoc,
             SolidElementIntersectionFactoryBase<Element> intersectionFactory)
        {
            _activeDoc = activeDoc;
            _intersectionFactory = intersectionFactory;
        }


        public ILogger Logger { get; set; }

        public IEnumerable<ElementId> GetIntersections(
            Element element)
        {

            var xYZIntersections = new List<ElementXYZIntersection>();

            Logger?.Information($"Try get : {element.Id} boundaries"); ;
            _intersectionFactory.ItemQuickFilters = [];
            var exclusionFilter = new ExclusionFilter(new List<ElementId>() { element.Id });
            _intersectionFactory.ItemQuickFilters.Add((exclusionFilter, null));

            var zoneSolid = GetZoneSolid(element);
            //if (zoneSolid != null)
            //{ ShowSolid(zoneSolid); }

            var intersections = _intersectionFactory.GetIntersections(zoneSolid);
            Logger?.Information("Intersections found: " + intersections.Count());
            
            return intersections.Select(e => e.Id);

            Solid GetZoneSolid(Element element)
            {
                var offsetDist = 0.001;
                double height;
                CurveLoop profile;
                switch (element)
                {
                    case Wall wall:
                        {
                            profile = wall.GetBottomProfile();
                            height = wall.GetHeigth();
                        }
                        break;
                    default:
                        { throw new NotImplementedException(); }
                }
                if (profile == null) { return null; }

                //ShowCurves(profile);
                //return null;

                profile = CurveLoop.CreateViaOffset(profile, offsetDist, -XYZ.BasisZ);

                return GeometryCreationUtilities
                   .CreateExtrusionGeometry(
                   new List<CurveLoop> { profile },
                   XYZ.BasisZ, height);
            }
        }

        private void ShowSolid(Solid solid)
        {
            using (Transaction transaction = new(_activeDoc, "ShowSolid"))
            {
                transaction.Start();
                solid.ShowShape(_activeDoc);

                if (transaction.HasStarted())
                { transaction.Commit(); }
            }

        }
    }
}
