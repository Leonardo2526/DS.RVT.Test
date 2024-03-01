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
            Func<EnergySurface, EnergySurface, bool> surfaceCondition = null,
            Func<Solid, bool> solidCondition = null)
        {
            if (surfaceCondition != null && !surfaceCondition(surface1, surface2))
            { return null; }

            var solidResult = BooleanOperationsUtils
              .ExecuteBooleanOperation(surface1.Solid, surface2.Solid, BooleanOperationsType.Intersect);
            return solidCondition == null || solidCondition(solidResult) ?
               surface1.Clone(solidResult) : null;
        }


        public static IEnumerable<EnergySurface> Intersections(
            IEnumerable<EnergySurface> surfaces1,
            IEnumerable<EnergySurface> surfaces2,
            Func<EnergySurface, EnergySurface, bool> surfaceCondition,
             Func<Solid, bool> solidCondition = null)
        {
            var results = new List<EnergySurface>();

            foreach (var surface1 in surfaces1)
            {
                foreach (var surface2 in surfaces2)
                {
                    var result = Intersection(surface1, surface2, surfaceCondition, solidCondition);
                    if (result != null)
                    { results.Add(result); }
                }
            }

            return results;
        }

        public static EnergySurface Difference(
           EnergySurface surface1,
           EnergySurface surface2,
           Func<EnergySurface, EnergySurface, bool> condition = null,
           Func<Solid, bool> solidCondition = null)
        {
            if (condition != null && !condition(surface1, surface2)) { return surface1; }
            var solidResult = BooleanOperationsUtils
                  .ExecuteBooleanOperation(surface1.Solid, surface2.Solid, BooleanOperationsType.Difference);
            return solidCondition == null || solidCondition(solidResult) ?
              surface1.Clone(solidResult) : null;
        }

        public static IEnumerable<EnergySurface> Differences(
          IEnumerable<EnergySurface> surfaces1,
          IEnumerable<EnergySurface> surfaces2,
          Func<EnergySurface, EnergySurface, bool> condition = null,
          Func<Solid, bool> solidCondition = null)
        {
            var results = new List<EnergySurface>();

            foreach (var surface1 in surfaces1)
            {
                var intersectionSolids = new List<Solid>();
                foreach (var surface2 in surfaces2)
                {
                    if (condition != null && !condition(surface1, surface2))
                    { continue; }
                    var intersectionSolid = BooleanOperationsUtils
                        .ExecuteBooleanOperation(surface1.Solid, surface2.Solid,
                        BooleanOperationsType.Intersect);
                    if (solidCondition == null || solidCondition(intersectionSolid))
                    { intersectionSolids.Add(intersectionSolid); }
                }

                var resultSolid = Autodesk.Revit.DB.SolidUtils.Clone(surface1.Solid);
                foreach (var solid in intersectionSolids)
                {
                    resultSolid = BooleanOperationsUtils
                        .ExecuteBooleanOperation(resultSolid, solid,
                        BooleanOperationsType.Difference);
                }
                if (solidCondition == null || solidCondition(resultSolid))
                { results.Add(surface1.Clone(resultSolid)); }
            }

            return results;
        }


        public static (EnergySurface result1, EnergySurface result2) SymmetricDifference(
          EnergySurface surface1,
          EnergySurface surface2,
          Func<Solid, bool> solidCondition = null)
        {
            var solidResult1 = BooleanOperationsUtils
                  .ExecuteBooleanOperation(surface1.Solid, surface2.Solid, BooleanOperationsType.Difference);
            var result1 = solidCondition == null || solidCondition(solidResult1) ?
                surface1.Clone(solidResult1) : null;

            var solidResult2 = BooleanOperationsUtils
                  .ExecuteBooleanOperation(surface2.Solid, surface1.Solid, BooleanOperationsType.Difference);
            var result2 = solidCondition == null || solidCondition(solidResult1) ?
                surface1.Clone(solidResult2) : null;

            return (result1, result2);
        }

        public static EnergySurface Union(
          EnergySurface surface1,
          EnergySurface surface2,
           Func<Solid, bool> solidCondition = null)
        {
            var solidResult = BooleanOperationsUtils
                  .ExecuteBooleanOperation(surface1.Solid, surface2.Solid, BooleanOperationsType.Union);
            return solidCondition == null || solidCondition(solidResult) ?
              surface1.Clone(solidResult) : null;
        }
    }
}
