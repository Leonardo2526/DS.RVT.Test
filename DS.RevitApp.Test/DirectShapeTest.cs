using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using OLMP.RevitAPI.Tools.Extensions;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    internal class DirectShapeTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        public DirectShapeTest(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
        }


        // Create a DirectShape Sphere
        public void CreateSphereDirectShape()
        {
            var profile = new List<Curve>();

            // first create sphere with 2' radius
            XYZ center = XYZ.Zero;
            double radius = 2.0;
            XYZ profile00 = center;
            XYZ profilePlus = center + new XYZ(0, radius, 0);
            XYZ profileMinus = center - new XYZ(0, radius, 0);

            profile.Add(Line.CreateBound(profilePlus, profileMinus));
            profile.Add(Arc.Create(profileMinus, profilePlus, center + new XYZ(radius, 0, 0)));

            CurveLoop curveLoop = CurveLoop.Create(profile);
            SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

            Frame frame = new Frame(center, XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);
            if (Frame.CanDefineRevitGeometry(frame) == true)
            {
                Solid sphere = GeometryCreationUtilities.CreateRevolvedGeometry(frame, new CurveLoop[] { curveLoop }, 0, 2 * Math.PI, options);
                using (Transaction t = new Transaction(_doc, "Create sphere direct shape"))
                {
                    t.Start();
                    // create direct shape and assign the sphere shape
                    DirectShape ds = DirectShape.CreateElement(_doc, new ElementId(BuiltInCategory.OST_GenericModel));

                    ds.ApplicationId = "Application id";
                    ds.ApplicationDataId = "Geometry object id";
                    ds.SetShape(new GeometryObject[] { sphere });
                    t.Commit();
                }
            }
        }

        public void SelectWall()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            var element = _doc.GetElement(reference);

            if (element is not Wall wall)
            { return; }

            var wallSolid = wall.Solid();

            using (Transaction t = new Transaction(_doc, "Create sphere direct shape"))
            {
                t.Start();
                ShowFace(wallSolid);
                t.Commit();
            }

        }

        public void Show(Solid solid)
        {
            using (Transaction t = new Transaction(_doc, "Create sphere direct shape"))
            {
                t.Start();
                // create direct shape and assign the sphere shape
                DirectShape ds = DirectShape.CreateElement(_doc, new ElementId(BuiltInCategory.OST_GenericModel));

                ds.ApplicationId = "Application id";
                ds.ApplicationDataId = "Geometry object id";
                ds.SetShape(new GeometryObject[] { solid });
                t.Commit();
            }
        }

        private void ShowFace(Solid solid)
        {

            var faces = new List<Face>();
            foreach (Face item in solid.Faces)
            {
                faces.Add(item);
            }
            faces = faces.OrderByDescending(f => f.Area).ToList();
            var face = faces.First();

            var mesh = face.Triangulate(1);
            ElementId categoryId = new ElementId(BuiltInCategory.OST_GenericModel);
            DirectShape ds = DirectShape.CreateElement(_doc, categoryId);
            ds.SetShape(new GeometryObject[] { mesh });

            //var tResult = Build_Tessellate(face);
            //var geom = tResult.GetGeometricalObjects();
            //ds.SetShape(geom);
        }

        private void ShowOld(Solid solid)
        {
            foreach (Face f in solid.Faces)
            {
                Build_Tessellate(f);
                ElementId categoryId = new ElementId(BuiltInCategory.OST_GenericModel);
                DirectShape ds = DirectShape.CreateElement(_doc, categoryId);
                ds.ApplicationId = System.Reflection.Assembly.GetExecutingAssembly().GetType().GUID.ToString();
                ds.ApplicationDataId = Guid.NewGuid().ToString();
                foreach (TessellatedShapeBuilderResult t1 in Build_Tessellate2(f))
                {
                    ds.SetShape(t1.GetGeometricalObjects());

                    ds.Name = "Single_Surface";
                }
            }
        }

        private TessellatedShapeBuilderResult Build_Tessellate(Face faces)
        {
            Mesh mesh = faces.Triangulate();

            TessellatedShapeBuilder builder = new TessellatedShapeBuilder();

            builder.OpenConnectedFaceSet(false);

            List<XYZ> args = new List<XYZ>(3);

            XYZ[] triangleCorners = new XYZ[3];

            for (int i = 0; i < mesh.NumTriangles; ++i)
            {
                MeshTriangle triangle = mesh.get_Triangle(i);

                triangleCorners[0] = triangle.get_Vertex(0);
                triangleCorners[1] = triangle.get_Vertex(1);
                triangleCorners[2] = triangle.get_Vertex(2);

                TessellatedFace tesseFace = new TessellatedFace(triangleCorners, ElementId.InvalidElementId);

                if (builder.DoesFaceHaveEnoughLoopsAndVertices(tesseFace))
                {
                    builder.AddFace(tesseFace);
                }
            }

            builder.CloseConnectedFaceSet();
            builder.Target = TessellatedShapeBuilderTarget.AnyGeometry;
            builder.Fallback = TessellatedShapeBuilderFallback.Mesh;

            builder.Build();

            TessellatedShapeBuilderResult result2 = builder.GetBuildResult();

            return result2;
        }

        private List<TessellatedShapeBuilderResult> Build_Tessellate2(Face faces)
        {
            Mesh mesh = faces.Triangulate();
            List<XYZ> vert = new List<XYZ>();

            foreach (XYZ ij in mesh.Vertices)
            {
                XYZ vertices = ij;
                vert.Add(vertices);
            }

            TessellatedShapeBuilder builder = new TessellatedShapeBuilder();

            builder.OpenConnectedFaceSet(false);

            //Filter for Title Blocks in active document
            FilteredElementCollector materials = new FilteredElementCollector(_doc)
            .OfClass(typeof(Autodesk.Revit.DB.Material))
            .OfCategory(BuiltInCategory.OST_Materials);

            ElementId materialId = materials.First().Id;

            builder.AddFace(new TessellatedFace(vert, materialId));

            builder.CloseConnectedFaceSet();
            builder.Target = TessellatedShapeBuilderTarget.AnyGeometry;
            builder.Fallback = TessellatedShapeBuilderFallback.Mesh;

            builder.Build();

            TessellatedShapeBuilderResult result3 = builder.GetBuildResult();
            List<TessellatedShapeBuilderResult> res = new List<TessellatedShapeBuilderResult>();

            if (result3.Outcome.ToString() == "Sheet")
            {
                res.Add(result3);
            }

            return res;
        }
    }
}
