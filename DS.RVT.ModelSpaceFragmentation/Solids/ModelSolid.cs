using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{


    class ModelSolid
    {
        readonly Document Doc;

        public ModelSolid(Document doc)
        {
            Doc = doc;
        }

        ElementUtils ElemUtils = new ElementUtils();

        public static Dictionary<Element, List<Solid>> modelSolids = new Dictionary<Element, List<Solid>>();

        public Dictionary<Element, List<Solid>> GetSolids()
        {
            FilteredElementCollector collector = new FilteredElementCollector(Doc);

            ICollection<BuiltInCategory> elementCategoryFilters = new List<BuiltInCategory>
                {
                    BuiltInCategory.OST_PipeCurves,
                    BuiltInCategory.OST_Walls
                };

            ElementMulticategoryFilter elementMulticategoryFilter = new ElementMulticategoryFilter(elementCategoryFilters);

            collector.WhereElementIsNotElementType();
            IList<Element> intersectedElementsBox = collector.WherePasses(elementMulticategoryFilter).ToElements();
            Dictionary<Element, List<Solid>> solidsDictionary = new Dictionary<Element, List<Solid>>();

            List<Solid> solids = new List<Solid>();
            foreach (Element elem in intersectedElementsBox)
            {
                solids = ElemUtils.GetSolids(elem);
                if (solids.Count !=0)
                {
                    modelSolids.Add(elem, solids);
                    solidsDictionary.Add(elem, solids);
                }             
            }

            return solidsDictionary;
        }


    }
}
