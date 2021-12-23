﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace DS.RVT.ModelSpaceFragmentation
{
    class BoundingBoxFilter
    {
        public BoundingBoxIntersectsFilter GetBoundingBoxFilter(IBoundingBoxFilter boundingBoxFilter)
        {
            return boundingBoxFilter.GetBoundingBoxFilter();
        }

    }

    interface IBoundingBoxFilter
    {
        BoundingBoxIntersectsFilter GetBoundingBoxFilter();
    }


    class LinesBoundingBox : IBoundingBoxFilter
    {
        public List<Line> Lines;

        public LinesBoundingBox (List<Line> lines)
        {
            Lines = lines;
        }

        /// <summary>
        /// Get bounding box by list of lines
        /// </summary>
        public BoundingBoxIntersectsFilter GetBoundingBoxFilter()
        {
            PointUtils pointUtils = new PointUtils();
            pointUtils.FindMinMaxPointByLines(Lines, out XYZ minPoint, out XYZ maxPoint);

            XYZ minRefPoint = new XYZ(minPoint.X, minPoint.Y, minPoint.Z);
            XYZ maxRefPoint = new XYZ(maxPoint.X, maxPoint.Y, maxPoint.Z);

            Outline myOutLn = new Outline(minRefPoint, maxRefPoint);

            return new BoundingBoxIntersectsFilter(myOutLn);
        }
    }

}
