using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCmd.EnergyTest.Energy
{
    internal readonly struct EnergyFace(
        Face face,
        IEnumerable<CompoundStructure> compoundStructures,
        EnergyAnalysisSurfaceType surfaceType)
    {
        public Face Face { get; } = face;
        public double Area => Face.Area;
        public IEnumerable<CompoundStructure> CompoundStructures { get; } = compoundStructures;
        public EnergyAnalysisSurfaceType SurfaceType { get; } = surfaceType;

        /// <summary>
        /// Heat transfer coefficient 
        /// </summary>
        public double H { get; } 

    }
}
