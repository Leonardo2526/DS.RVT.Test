using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Mechanical;
using DS.ClassLib.VarUtils;
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
        private Face _face;

        public EnergySurface(Solid analitycalSolid, Element hostElement, IEnumerable<EnergySurface> inserts = null)
        {
            Solid = analitycalSolid;
            Host = hostElement;
            Inserts = inserts ?? new List<EnergySurface>();
        }

        public Solid Solid { get; private set; }

        public Element Host { get; }

        public Face Face => _face ??=
            Solid.Faces.ToList().OrderByDescending(f => f.Area).First();

        public double Area => Face.Area;

        public EnergyAnalysisSurfaceType SurfaceType { get; set; }

        /// <summary>
        /// Heat transfer coefficient 
        /// </summary>
        public double H { get; set; }

        public IEnumerable<EnergySurface> Inserts { get; }

        public EnergySurface Clone()
        {
            var solid = Autodesk.Revit.DB.SolidUtils.Clone(Solid);
            return new(solid, Host, Inserts)
            {
                H = H,
                SurfaceType = SurfaceType
            };
        }

        public EnergySurface Clone(Solid solid)
        {
            var inserts = GetInsertes(solid);
            return new(solid, Host, inserts)
            {
                H = H,
                SurfaceType = SurfaceType
            };
        }

        public void Show(Document activeDoc)
            => Solid.ShowShape(activeDoc); 

        private IEnumerable<EnergySurface> GetInsertes(Solid hostSolid)
        {
            var inserts = new List<EnergySurface>();
            foreach (var insert in Inserts)
            {
                var solidResult = BooleanOperationsUtils
                    .ExecuteBooleanOperation(hostSolid, insert.Solid, BooleanOperationsType.Intersect);
                if(MinVolumeCondition(solidResult))
                { inserts.Add(insert); }

            }
            return inserts;

            static bool MinVolumeCondition(Solid solid)
            {
                double minIntersectionVolume = 1.CMToFeet(3);
                return solid != null && Math.Abs(solid.Volume) > minIntersectionVolume;
            }
        }
    }
}
