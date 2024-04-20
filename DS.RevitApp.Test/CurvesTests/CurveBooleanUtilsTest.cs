using Autodesk.Revit.DB;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.CurvesTests
{
    internal class CurveBooleanUtilsTest
    {

        /// <summary>
        /// Find intersection between <paramref name="curve1"/> ands <paramref name="curve2"/>.
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <param name="tolerance"></param>
        /// <returns>
        /// A new <see cref="Curve"/> that is result of <paramref name="curve2"/> overlaing on <paramref name="curve1"/>.
        /// <para>
        /// <see langword="null"/> if curves are not overlaing.
        /// </para>
        /// </returns>
        public static Curve Intersect(Curve curve1, Curve curve2, double tolerance = RhinoMath.ZeroTolerance)
        {
            var p21 = curve2.GetEndPoint(0);
            var p22 = curve2.GetEndPoint(1);

            if(!curve1.Contains(p21) && !curve1.Contains(p22)) 
            { return null; }

            var p21Proj = curve1.Project(p21);
            var p22Proj = curve1.Project(p22);

            var resultCurve = curve1.Clone();
            resultCurve.MakeBound(p21Proj.XYZPoint, p22Proj.XYZPoint);
            return resultCurve;
        }
    }


}
