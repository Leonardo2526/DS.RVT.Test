using Autodesk.Revit.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    internal static class NewExtensions
    {
        public static IEnumerable<Face> ToList(this FaceArray faceArray)
        {
            var faces = new List<Face>();
            foreach (Face face in faceArray)
            { faces.Add(face); }
            return faces;
        }

        public static IEnumerable<T> AsEnumerable<T>(this IEnumerable resultArray)
        {
            var intersectionResults = new List<T>();
            foreach (T intersectionResult in resultArray)
            { intersectionResults.Add(intersectionResult); }
            return intersectionResults;
        }
    }
}
