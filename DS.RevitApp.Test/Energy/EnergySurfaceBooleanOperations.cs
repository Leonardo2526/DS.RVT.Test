using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace DS.RevitApp.Test.Energy
{
    public static class EnergySurfaceBooleanOperations
    {
        public static EnergySurface Intersection(
            EnergySurface surface1,
            EnergySurface surface2,
            double minIntersectionVolume = 0)
        {
            var solidResult = BooleanOperationsUtils
                  .ExecuteBooleanOperation(surface1.Solid, surface2.Solid, BooleanOperationsType.Intersect);
            return solidResult != null && Math.Abs(solidResult.Volume) > minIntersectionVolume ?
              surface1.Clone(solidResult) : null;
        }


        public static IEnumerable<EnergySurface> Intersections(
            IEnumerable<EnergySurface> surfaces1,
            IEnumerable<EnergySurface> surfaces2,
            double minIntersectionVolume = 0)
        {
            var results = new List<EnergySurface>();

            foreach (var surface1 in surfaces1)
            {
                foreach (var surface2 in surfaces2)
                {
                    var result = Intersection(surface1, surface2, minIntersectionVolume);
                    if (result != null) results.Add(result);
                }
            }

            return results;
        }

        public static EnergySurface Difference(
           EnergySurface surface1,
           EnergySurface surface2,
           double minIntersectionVolume = 0)
        {
            var solidResult = BooleanOperationsUtils
                  .ExecuteBooleanOperation(surface1.Solid, surface2.Solid, BooleanOperationsType.Difference);
            return solidResult != null && Math.Abs(solidResult.Volume) > minIntersectionVolume ?
              surface1.Clone(solidResult) : null;
        }

        public static IEnumerable<EnergySurface> Differences(
          IEnumerable<EnergySurface> surfaces1,
          IEnumerable<EnergySurface> surfaces2,
          double minIntersectionVolume = 0)
        {
            var results = new List<EnergySurface>();

            foreach (var surface1 in surfaces1)
            {
                var intersectionSolids = new List<Solid>();
                foreach (var surface2 in surfaces2)
                {
                    var intersectionSolid = 
                        GetIntersetionSolid(surface1.Solid, surface2.Solid, minIntersectionVolume);
                    if (intersectionSolid != null) intersectionSolids.Add(intersectionSolid);
                }

                var resultSolid = Autodesk.Revit.DB.SolidUtils.Clone(surface1.Solid);
                foreach (var solid in intersectionSolids)
                { resultSolid = GetDifferenceSolid(resultSolid, solid, minIntersectionVolume); }
                results.Add(surface1.Clone(resultSolid));
            }

            return results;
        }


        public static (EnergySurface result1, EnergySurface result2) SymmetricDifference(
          EnergySurface surface1,
          EnergySurface surface2,
          double minIntersectionVolume = 0)
        {
            var solidResult1 = BooleanOperationsUtils
                  .ExecuteBooleanOperation(surface1.Solid, surface2.Solid, BooleanOperationsType.Difference);
            var result1 = solidResult1 != null && Math.Abs(solidResult1.Volume) > minIntersectionVolume ?
                surface1.Clone(solidResult1) : null;

            var solidResult2 = BooleanOperationsUtils
                  .ExecuteBooleanOperation(surface2.Solid, surface1.Solid, BooleanOperationsType.Difference);
            var result2 = solidResult2 != null && Math.Abs(solidResult2.Volume) > minIntersectionVolume ?
                surface1.Clone(solidResult2) : null;

            return (result1, result2);
        }

        public static EnergySurface Union(
          EnergySurface surface1,
          EnergySurface surface2,
          double minIntersectionVolume = 0)
        {
            var solidResult = BooleanOperationsUtils
                  .ExecuteBooleanOperation(surface1.Solid, surface2.Solid, BooleanOperationsType.Union);
            return solidResult != null && Math.Abs(solidResult.Volume) > minIntersectionVolume ?
              surface1.Clone(solidResult) : null;
        }

       private static Solid GetDifferenceSolid(
           Solid solid1,
           Solid solid2,
           double minIntersectionVolume)
        {
            var solidResult = BooleanOperationsUtils
                 .ExecuteBooleanOperation(solid1, solid2, BooleanOperationsType.Difference);
            return solidResult != null && Math.Abs(solidResult.Volume) > minIntersectionVolume ?
              solidResult : null;
        }

        private static Solid GetIntersetionSolid(
           Solid solid1,
           Solid solid2,
            double minIntersectionVolume)
        {
            var solidResult = BooleanOperationsUtils
                 .ExecuteBooleanOperation(solid1, solid2, BooleanOperationsType.Intersect);
            return solidResult != null && Math.Abs(solidResult.Volume) > minIntersectionVolume ?
              solidResult : null;
        }
    }
}
