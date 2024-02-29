using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using MoreLinq;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Solids;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.Energy
{
    public class EnergySurface
    {
        private readonly static double _mmToFeet =
            Rhino.RhinoMath.UnitScale(Rhino.UnitSystem.Millimeters, Rhino.UnitSystem.Feet);

        private readonly static double _cmToFeet =
         Rhino.RhinoMath.UnitScale(Rhino.UnitSystem.Centimeters, Rhino.UnitSystem.Feet);
        private readonly static double _cubicCMToFeet = Math.Pow(_cmToFeet, 3);

        private readonly static double _minIntersectionVolume = 1 * _cubicCMToFeet;

        public EnergySurface(Solid analitycalSolid, Element hostElement)
        {
            Solid = analitycalSolid;
            Host = hostElement;
            var faces = analitycalSolid.Faces.ToList();
            faces = faces.OrderByDescending(f => f.Area).ToList();
            Face = faces.First();
        }

        public Solid Solid { get; private set; }

        public Element Host { get; }

        public Face Face { get; private set; }

        public double Area => Face.Area;

        public EnergyAnalysisSurfaceType SurfaceType { get; set; }

        /// <summary>
        /// Heat transfer coefficient 
        /// </summary>
        public double H { get; set; }


        public EnergySurface Clone()
        {
            var solid = Autodesk.Revit.DB.SolidUtils.Clone(Solid);
            return new(solid, Host)
            {
                Face = Face,
                H = H,
                SurfaceType = SurfaceType
            };
        }

        public EnergySurface Clone(Solid solid)
       => new(solid, Host)
       {
           Face = Face,
           H = H,
           SurfaceType = SurfaceType
       };

        public void Cut(IEnumerable<Solid> solids)
        {
            foreach (var solid in solids)
            {
                Solid = BooleanOperationsUtils
                    .ExecuteBooleanOperation(Solid, solid, BooleanOperationsType.Difference);
            }
        }

        public EnergySurface GetIntersectionSurface(Solid solid)
        {
            var intersection = BooleanOperationsUtils
                       .ExecuteBooleanOperation(Solid, solid, BooleanOperationsType.Intersect);
            return intersection != null && Math.Abs(intersection.Volume) > _minIntersectionVolume ?
                Clone(intersection) : null;
        }
    }
}
