using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace DS.RevitApp.Test.Energy
{
    internal class AdjacenciesFuncBuilder
    {
        public Func<Space, IEnumerable<Space>> Create()
        {
            return null;
        }


        private IEnumerable<Space> GetAdjacencies(Space energyModel)
        {
            var adjacencies = new List<Space>();

            var box = GetBoundingBoxXYZ(energyModel);
            var items = GetBoundingBoxItems(box);
            adjacencies.AddRange(items);

            return adjacencies;
        }

        private BoundingBoxXYZ GetBoundingBoxXYZ(Space energySpace)
        {
            return null;
            //var box = energySpace.EnergySpace.Space.get_BoundingBox(null);


            //double offset = 5;
            //var outline = box.GetOutline();

            //var diagonal = new Rhino.Geometry.Line(outline.MinimumPoint.ToPoint3d(), outline.MaximumPoint.ToPoint3d());
            //var sourceLength = diagonal.Length;
            //if (!diagonal.Extend(sourceLength, sourceLength + offset * 2))
            //{ throw new Exception(); }
            //var targetLength = diagonal.Length;

            //var scale = targetLength / sourceLength;
            //outline.Scale(scale); outline.GetBoundingBoxFilter(_links);
            //var scaleBox = new BoundingBoxXYZ()
            //{ Min = outline.MinimumPoint, Max = outline.MaximumPoint };

            //return scaleBox;
        }

        private IEnumerable<Space> GetBoundingBoxItems(BoundingBoxXYZ boxXYZ)
        {
            var spaces = new List<Space>();

            return spaces;
        }
    }
}
