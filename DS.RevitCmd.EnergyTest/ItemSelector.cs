using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace DS.RevitCmd.EnergyTest
{
    internal class ItemSelector
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;

        public ItemSelector(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
        }


        public Wall SelectWall()
        {
            Reference reference = _uiDoc.Selection
                      .PickObject(ObjectType.Element, $"Select wall");
            return _doc.GetElement(reference) as Wall;
        }

        public Face SelectFace()
        {
            Reference faceRef = _uiDoc.Selection
                      .PickObject(ObjectType.Face, $"Select face");

            var geoObject = _doc.GetElement(faceRef).GetGeometryObjectFromReference(faceRef);
            return geoObject as Face;
        }
    }
}
