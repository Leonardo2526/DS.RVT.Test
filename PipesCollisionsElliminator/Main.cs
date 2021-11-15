using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
namespace DS.PipesCollisionsElliminator
{
    class Main
    {
        readonly Application App;
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public Main(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }

        public void GetElementInfo()
        {
            ElementUtils elementUtils = new ElementUtils();
            Element element = elementUtils.GetCurrent(new PickedElement(Uidoc, Doc));

            elementUtils.OutputInfo(new ElementInfo(element, elementUtils));

        }

        public void InitiateProcess()
        {
            ElementUtils elementUtils = new ElementUtils();
            elementUtils.GetCurrent(new PickedElement(Uidoc, Doc));
            

        }




    }
}
