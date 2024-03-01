using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Mechanical;
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

        public void Show(Document activeDoc)
            => Solid.ShowShape(activeDoc);        
    }
}
