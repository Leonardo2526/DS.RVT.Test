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
    class ElementInfo : IOutputElementInfo
    {
        readonly Element element;
        readonly ElementUtils elementUtils;

        public ElementInfo (Element elem, ElementUtils elemUtils)
        {
            element = elem;
            elementUtils = elemUtils;
        }

        public void GetInfo()        
        {
            XYZ pointInMM = elementUtils.GetLocationInMM(element);

            string output = $"Element category: {element.Category} " +
                $"\n Element name: {element.Name} " +
                $"\n Element ID: {element.Id} " +
                $"\n Element location in mm: ({pointInMM.X}, {pointInMM.Y}, {pointInMM.Z})";

            TaskDialog.Show("Revit", output);
        }



    }
}
